﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace MyApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {

        static readonly string[] scopeRequiredByApiForReadData = new string[] { "users.read" };


        private readonly ITokenAcquisition _tokenAcquisition;

        public UsersController(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        [HttpGet]
        public async Task<IEnumerable<UserModel>> Get()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApiForReadData);


            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "User.ReadBasic.All" });

            var graphServiceClient = new GraphServiceClient(
                        new DelegateAuthenticationProvider((requestMessage) =>
                        {
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                            return Task.CompletedTask;
                        }));


            var userList = await graphServiceClient.Users.Request().GetAsync();

            return userList.Select(x => new UserModel()
            {
                Name = x.DisplayName
            }).ToList();
        }
    }
}
