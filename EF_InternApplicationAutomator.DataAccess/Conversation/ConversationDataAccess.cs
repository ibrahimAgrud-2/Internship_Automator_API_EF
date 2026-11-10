using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application;
using Shared.Intern;
using Shared.Message;


namespace EF_InternApplicationAutomator.DataAccess.Conversation
{
    public class ConversationDataAccess
    {

        
        private readonly IAADbContext _Context;
        public ConversationDataAccess(IAADbContext context)
        {
            _Context = context;
        }
        public List<ConversationResponseDTO> GetAllConversation()
        {

            return _Context.Conversation
                .AsNoTracking()
                .Select(c => new ConversationResponseDTO(c.ConversationID,c.ConversationName,c.IsGroup))
                .ToList();
        }

        public List<MessageDetailedResponseDTO> GetAllMessages(int conversationID)
        {
            return _Context.vw_MessagesWithContent.Where(m=>m.ConversationID==conversationID).Select(m => new MessageDetailedResponseDTO(m.MessageID,m.PersonFullName,m.ConversationID,m.Content,m.CreatedDate,m.IsStaff,m.ReferencedContentID,m.ReferencedContentType,m.ReferencedContentTitle))
           .ToList();
        }


    }
}
