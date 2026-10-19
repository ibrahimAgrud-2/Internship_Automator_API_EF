

using EF_InternApplicationAutomator.DataAccess.Conversation;
using Shared.Conversations;
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
    }
}