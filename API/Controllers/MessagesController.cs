using API.Controllers.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this._userRepository = userRepository;
            this._messageRepository = messageRepository;
            this._mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.getUserName();

            if(username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You can't send yourself a message");
            
            var sender = await _userRepository.GetUserByUsernameAsync(username);

            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient == null)
                return NotFound();
           
            var message = new Message{
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("fails to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.getUserName();

            var messages = await _messageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader
            (messages.CurrentPage, messages.PageSize, messages.TotalCount,messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> getMessageThread(string username)
        {
            var currentUsername = User.getUserName();

            return Ok(await _messageRepository.GetMessageThread(currentUsername,username));
        }

    }
}