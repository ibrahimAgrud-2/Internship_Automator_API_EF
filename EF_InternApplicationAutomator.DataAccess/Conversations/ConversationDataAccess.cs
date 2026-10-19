

using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.Messages;
using Microsoft.EntityFrameworkCore;
using Shared.Conversations;
using Shared.Message;
using System.Collections.Generic;

namespace EF_InternApplicationAutomator.DataAccess.Conversation
{
    public class ConversationDataAccess
    {
        private readonly IAADbContext _Context;

        public ConversationDataAccess(IAADbContext context)
        {
            _Context = context;
        }

        public List<ConversationResponseDTO> GetAll()
        {
            List<ConversationEntity> ce= _Context.Conversations.ToList();

            List<ConversationResponseDTO> conversationResponseDTOs = new List<ConversationResponseDTO>();

            foreach (var item in ce)
            {
                conversationResponseDTOs.Add(new ConversationResponseDTO(item.ConversationID,item.InternID,item.ConversationName,item.IsGroup));
            }
            return conversationResponseDTOs;
        }



        public List<MessageResponseDTO> GetAllMessagesForConversation(int conversationID,int messageCount=50)
        {
            List<MessageEntity> ce = _Context.Messages.Include(p=>p.Person).OrderBy(m => m.MessageID)
                     .Take(messageCount)
                     .ToList();

            List<MessageResponseDTO> conversationResponseDTOs = new List<MessageResponseDTO>();

            foreach (var item in ce)
            {
                conversationResponseDTOs.Add(new MessageResponseDTO(item.MessageID, item.PersonID, item.ConversationID, item.Content, item.CreatedDate, item.IsStaff,$"{item.Person.FirstName} {item.Person.LastName}"));
            }
            return conversationResponseDTOs;
        }
    }
}