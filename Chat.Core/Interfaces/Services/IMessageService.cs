using Chat.Common.DTOs;
using System.Threading.Tasks;

namespace Chat.Core.Interfaces.Services
{
    public interface IMessageService
    {
        Task CreateMessage(MessageCreateDTO messageDTO, string userId);
    }
}
