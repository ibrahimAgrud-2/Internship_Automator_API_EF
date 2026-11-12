
using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.ConversationIntern;
using EF_InternApplicationAutomator.DataAccess.Message;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared;
using Shared.Application;
using Shared.Intern;
using Shared.Intern.ConversationIntern;
using Shared.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.Business.ConversationIntern
{
    public class ConversationInternBL
    {

        public enum enStatus { newApp=0,Cancelled=1,Declined=2,Accepted=3};


         ConversationInternDataAccess _ConversationInternDataAccess;

        public ConversationInternBL(ConversationInternDataAccess conversationInternDataAccess)
        {
            _ConversationInternDataAccess = conversationInternDataAccess;
    
        }

        public List<ConversationInternResponseDTO> GetAllInternConversation(int internID)
        {
            return _ConversationInternDataAccess.GetAllInternConversation(internID);
        }
       
    }
}