using API.Controllers.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this._mapper = mapper;
            _context = context;
            
        }
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int Id)
        {
            return await _context.Messages.FindAsync(Id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages
            .OrderByDescending(m=> m.MessageSent)
            .AsQueryable();

            query = messageParams.Containner switch{
                "Inbox" => query.Where(u=> u.RecipientUsername == messageParams.Username),
                "Outbox" => query.Where(u=> u.SenderUsername == messageParams.Username),
                _ => query.Where(u=> u.RecipientUsername == messageParams.Username && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>
            .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientusername)
        {
           /* The code is retrieving a list of messages from the database. It uses the
           `_context.Messages` property to access the `Messages` table in the database. */
            var messages = await _context.Messages
                .Include(u=> u.Sender).ThenInclude(p=> p.Photos)
                .Include(u=> u.Recipient).ThenInclude(p=> p.Photos)
                .Where(
                    m=> m.RecipientUsername == currentUsername 
                    && m.SenderUsername == recipientusername ||
                    m.RecipientUsername == recipientusername &&
                    m.SenderUsername == currentUsername
                )
                .OrderByDescending(m=> m.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(m=> m.DateRead == null &&
                m.RecipientUsername == currentUsername).ToList();

            /* The code block is checking if there are any unread messages in the `unreadMessages`
            list. If there are unread messages, it iterates through each message in the list and
            sets the `DateRead` property to the current date and time using `DateTime.Now`. After
            updating the `DateRead` property for all unread messages, it saves the changes to the
            database using `_context.SaveChangesAsync()`. This ensures that the `DateRead` property
            is updated for all unread messages in the database. */
            if(unreadMessages.Any()){
                foreach(var message in unreadMessages){
                    message.DateRead = DateTime.Now;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}