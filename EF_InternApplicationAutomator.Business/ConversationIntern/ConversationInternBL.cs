using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.ConversationInterns;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using Shared.Intern;

namespace EF_InternApplicationAutomator.Business.Intern
{
    public class ConversationInternBL
    {
        private readonly ConversationInternsDataAccess _ConversationInternsDataAccess;
        private readonly ConversationDataAccess _ConversationDataAccess;


        public ConversationInternBL(ConversationInternsDataAccess conversationInternsDataAccess, ConversationDataAccess conversationDataAccess)
        {
            _ConversationInternsDataAccess = conversationInternsDataAccess;
            _ConversationDataAccess = conversationDataAccess;


        }





    }
}