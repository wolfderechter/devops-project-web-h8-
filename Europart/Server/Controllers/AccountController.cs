﻿using EuropArt.Shared.Accounts;
using EuropArt.Shared.Artists;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EuropArt.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private IAccountService accountService;
        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost]
        public Task AddLikeAsync([FromBody] AccountRequest.AddLike request)
        {
            return accountService.AddLikeAsync(request);
        }

        [HttpGet("{AuthId}")]
        public Task<AccountResponse.GetLikes> GetLikesAsync([FromRoute] AccountRequest.GetLikes request)
        {
            return accountService.GetLikesAsync(request);
        }

        [HttpDelete("{AuthId}/{ArtworkId}")]
        public Task DeleteAsync([FromRoute] AccountRequest.DeleteLike request)
        {
            return accountService.DeleteLikeAsync(request);
        }

        [HttpGet("messages/{AuthId}")]
        public Task<AccountResponse.GetConversations> GetConversationsAsync([FromRoute] AccountRequest.GetConversations request)
        {
            return accountService.GetConversationsAsync(request);
        }

        [HttpGet("index/{AuthId}")]
        public Task<AccountResponse.GetIndex> GetIndexAsync([FromRoute] AccountRequest.GetIndex request)
        {
            return accountService.GetIndexAsync(request);
        }

        [HttpPost("AddMessageAsync")]
        public Task AddMessageAsync([FromBody] AccountRequest.AddMessage request)
        {
            return accountService.AddMessageAsync(request);
        }

        [HttpPost("AddConversationAsync")]
        public Task StartConversationAsync([FromBody] AccountRequest.StartConversation request)
        {
            return accountService.StartConversationAsync(request);
        }

    }
}
