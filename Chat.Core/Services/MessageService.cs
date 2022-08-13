﻿using Chat.Common.DTOs;
using Chat.Core.ChatTypesEnum;
using Chat.Core.Exceptions;
using Chat.Core.Interfaces.Services;
using Chat.Infrastructure.Entities;
using Chat.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Chat.Core.Services
{
    public class MessageService: IMessageService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<PrivateChat> _privateChatRepository;
        private readonly IRepository<Message> _messageRepository;
        public MessageService(
            UserManager<User> userManager,
            IRepository<Group> groupRepository,
            IRepository<PrivateChat> privateChatRepository,
            IRepository<Message> messageRepository
            )
        {
            _userManager = userManager;
            _groupRepository = groupRepository;
            _privateChatRepository = privateChatRepository;
            _messageRepository = messageRepository;
        }

        public async Task CreateMessage(MessageCreateDTO messageDTO, string userId)
        {
            var user = _userManager.FindByIdAsync(userId);
            
            if (user == null)
                throw new HttpException(System.Net.HttpStatusCode.NotFound, "User not found");

            if (messageDTO.ChatType == (int)ChatTypes.GroupChat)
            {
                var chat = await _groupRepository.GetByKeyAsync(messageDTO.ChatId)
                    ?? throw new HttpException(System.Net.HttpStatusCode.NotFound, "Chat not found");
            }
            else
            {
                var chat = await _privateChatRepository.GetByKeyAsync(messageDTO.ChatId)
                    ?? throw new HttpException(System.Net.HttpStatusCode.NotFound, "Chat not found");
            }

            var message = new Message()
            {
                Text = messageDTO.Text,
                CreatedAt = DateTime.Now,
                SenderId = userId,
                GroupId = (messageDTO.ChatType == (int)ChatTypes.GroupChat) ? messageDTO.ChatId : null,
                PrivateChatId = (messageDTO.ChatType == (int)ChatTypes.PrivateChat) ? messageDTO.ChatId : null
            };

            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();
        }
    }
}
