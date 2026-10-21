

using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Conversations;
using Shared.Intern;
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




        public List<vw_MessagesWithContentDTO> GetAllMessagesForConversation(int conversationID, int messageCount = 50)
        {
            List<vw_MessagesWithContent> ce = _Context.MessageDetailedView.Take(messageCount).ToList();

            List<vw_MessagesWithContentDTO> conversationResponseDTOs = new List<vw_MessagesWithContentDTO>();

          

            foreach (var item in ce)
            {
                conversationResponseDTOs.Add(new vw_MessagesWithContentDTO(item.MessageID,item.PersonID,item.ConversationID,item.Content,item.CreatedDate,item.IsStaff,item.ReferencedContentID,item.ReferencedContentType,item.ReferencedContentTitle));
            }
            return conversationResponseDTOs;
        }


        public bool IsConversationExists(int conversationID)
        {
            return _Context.Conversations.Any(c=>c.ConversationID==conversationID);
        }

    }
}