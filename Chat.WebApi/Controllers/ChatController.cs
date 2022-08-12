using Chat.Common.DTOs;
using Chat.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Chat.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(
            IChatService chatService)
        {
            _chatService = chatService;
        }
        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier).Value;

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroup(GroupCreateDTO groupCreateDTO)
        {
            await _chatService.CreateGroup(groupCreateDTO, UserId);

            return Ok();
        }

        [HttpPost("privateChat")]
        public async Task<IActionResult> CreatePrivateChat(PrivateChatCreateDTO privateChatCreateDTO)
        {
            await _chatService.CreatePrivateChat(privateChatCreateDTO, UserId);

            return Ok();
        }

        [HttpGet("chats")]
        public async Task<IActionResult> GetChats()
        {
            var chats = await _chatService.GetAllChats(UserId);
            
            return Ok(chats);
        }
    }
}
