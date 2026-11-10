
using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared;
using Shared.Application;
using Shared.Intern;
using Shared.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.Business.Conversation
{
    public class ConversationBL
    {

        public enum enStatus { newApp=0,Cancelled=1,Declined=2,Accepted=3};


         ConversationDataAccess _ConversationDataAccess;

        public ConversationBL(ConversationDataAccess conversationDataAccess)
        {
            _ConversationDataAccess = conversationDataAccess;
        }

        public List<ConversationResponseDTO>GetAllConversation()
        {
            return _ConversationDataAccess.GetAllConversation();
        }
        public List<MessageDetailedResponseDTO> GetAllMessages(int conversationID)
        {
            return _ConversationDataAccess.GetAllMessages(conversationID);
        }
    }
}