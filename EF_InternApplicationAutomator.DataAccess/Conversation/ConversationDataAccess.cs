using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Data;
using EF_InternApplicationAutomator.DataAccess.Message;
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

            return _Context.Conversations
                .AsNoTracking()
                .Select(c => new ConversationResponseDTO(c.ConversationID,c.ConversationName,c.IsGroup))
                .ToList();
        }

        public ConversationResponseDTO? Find(int ID)
        {
            ConversationEntity? en = _Context.Conversations.FirstOrDefault(c=>c.ConversationID==ID);

            if(en==null)
            {
                return null;
            }
            return new ConversationResponseDTO(en.ConversationID,en.ConversationName,en.IsGroup);
           
        }

        public List<MessageDetailedResponseDTO> GetAllMessages(int conversationID)
        {
            return _Context.vw_MessagesWithContent.Where(m=>m.ConversationID==conversationID).Select(m => new MessageDetailedResponseDTO(m.MessageID,m.PersonFullName,m.ConversationID,m.Content,m.CreatedDate,m.IsStaff,m.ReferencedContentID,m.ReferencedContentType,m.ReferencedContentTitle))
           .ToList();
        }

        public bool IsInternExistsInConversations(int internID, int conversationID)
        {
            return _Context.ConversationIntern.Any(c => c.InternID == internID && c.ConversationID == conversationID);


        }

        public bool IsConversationExists(int conversationID)
        {
            return _Context.Conversations.Any(c => c.ConversationID == conversationID);
        }


    }
}
