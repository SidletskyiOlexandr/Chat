using AutoMapper;
using Chat.Common.DTOs;
using Chat.Core.Exceptions;
using Chat.Core.Interfaces.Services;
using Chat.Infrastructure.Entities;
using Chat.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Core.ChatTypesEnum;
using System;

namespace Chat.Core.Services
{
    public class ChatService : IChatService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserGroup> _userGroupRepository;
        private readonly IRepository<UserPrivateChat> _userPrivateChatRepository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<PrivateChat> _privateChatRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        //private readonly ChatTypes chatTypes = ChatTypes.

        public ChatService(
            UserManager<User> userManager,
            IRepository<User> userRepository,
            IRepository<UserGroup> userGroupRepository,
            IRepository<UserPrivateChat> userPrivateChatRepository,
            IRepository<Group> groupRepository,
            IRepository<PrivateChat> privateChatRepository,
            IMapper mapper)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
            _userPrivateChatRepository = userPrivateChatRepository;
            _groupRepository = groupRepository;
            _privateChatRepository = privateChatRepository;
            _mapper = mapper;
        }
        public async Task CreateGroup(GroupCreateDTO groupCreateDTO, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
                throw new HttpException(System.Net.HttpStatusCode.NotFound, "User not found");

            var group = new Group()
            {
                Name = groupCreateDTO.Name,
                Description = groupCreateDTO.Description,
                CreatorId = userId
            };

            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveChangesAsync();

            var userGroup = new UserGroup()
            {
                GroupId = group.Id,
                UserId = userId
            };

            await _userGroupRepository.AddAsync(userGroup);
            await _userGroupRepository.SaveChangesAsync();

            await Task.CompletedTask;
        }

        public async Task CreatePrivateChat(PrivateChatCreateDTO privateChatCreateDTO, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var toUser = await _userManager.FindByEmailAsync(privateChatCreateDTO.ToUserEmail);
            if (user == null || toUser == null)
                throw new HttpException(System.Net.HttpStatusCode.NotFound, "User not found");

            var privateChat = new PrivateChat()
            {
                Name = privateChatCreateDTO.Name
            };

            await _privateChatRepository.AddAsync(privateChat);
            await _privateChatRepository.SaveChangesAsync();

            var userPrivateChat = new UserPrivateChat()
            {
                PrivateChatId = privateChat.Id,
                UserId = user.Id
            };

            await _userPrivateChatRepository.AddAsync(userPrivateChat);
            await _userPrivateChatRepository.SaveChangesAsync();

            await Task.CompletedTask;
        }

        public async Task<List<ChatInfoDTO>> GetAllChats(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var groups = await _userGroupRepository.Query()
                .Where(x => x.UserId == userId)
                .Include(x => x.Group)
                    .ThenInclude(x => x.Messages)
                    .Select(x => new ChatInfoDTO()
                    {
                        Id = x.GroupId,
                        Name = x.Group.Name,
                        //ChatTypeId = Enum.TryParse(ChatTypes.GroupChat, out int),
                        ChatTypeId = (int)ChatTypes.GroupChat,
                        LastMessageText = (x.Group.Messages.Count>0)? 
                            x.Group.Messages
                                .OrderByDescending(y => y.CreatedAt)
                                .First()
                                .Text 
                                : "",
                        
                        LastMessageCreatedAt = (x.Group.Messages.Count > 0) 
                            ? x.Group.Messages.
                            OrderByDescending(y => y.CreatedAt)
                            .First()
                            .CreatedAt: 
                            x.Group.CreatedAt 
                    })
                .ToListAsync();

            var privateChats = await _userPrivateChatRepository.Query()
                .Where(x => x.UserId == userId)
                .Include(x => x.PrivateChat)
                    .ThenInclude(x => x.Messages)
                    .Select(x => new ChatInfoDTO()
                    {
                        Id = x.PrivateChatId,
                        Name = x.PrivateChat.Name,
                        ChatTypeId = (int)ChatTypes.PrivateChat,
                        LastMessageText = (x.PrivateChat.Messages.Count > 0) ?
                            x.PrivateChat.Messages
                                .OrderByDescending(y => y.CreatedAt)
                                .First()
                                .Text
                                : "",

                        LastMessageCreatedAt = (x.PrivateChat.Messages.Count > 0)
                            ? x.PrivateChat.Messages.
                            OrderByDescending(y => y.CreatedAt)
                            .First()
                            .CreatedAt :
                            x.PrivateChat.CreatedAt
                    })
                .ToListAsync();

            groups.AddRange(privateChats);
            groups.OrderBy(x => x.LastMessageCreatedAt);

            return groups;
        }
    }
}
