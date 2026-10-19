using EF_InternApplicationAutomator.DataAccess.Conversation;
using Shared.Conversations;
using Shared.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Messages
{
    public class MessageDataAccess
    {
        private readonly IAADbContext _Context;

        public MessageDataAccess(IAADbContext context)
        {
            _Context = context;
        }
        public List<MessageResponseDTO> GetAllMessagesForConversation(int conversationID)
        {
            List<MessageEntity> ce = _Context.Messages.Where(m => m.ConversationID == conversationID).ToList();

            List<MessageResponseDTO> conversationResponseDTOs = new List<MessageResponseDTO>();

            foreach (var item in ce)
            {
                conversationResponseDTOs.Add(new MessageResponseDTO(item.MessageID,item.PersonID,item.ConversationID,item.Content,item.CreatedDate));
            }
            return conversationResponseDTOs;
        }
    }
}
