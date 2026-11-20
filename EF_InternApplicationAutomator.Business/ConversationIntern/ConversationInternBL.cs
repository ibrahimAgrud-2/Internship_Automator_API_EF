
using EF_InternApplicationAutomator.Business.Conversation;
using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.ConversationIntern;
using EF_InternApplicationAutomator.DataAccess.Message;
using Microsoft.EntityFrameworkCore;
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
         InternBL _InternBL;
           ConversationBL _Conversation;

        public ConversationInternBL(ConversationInternDataAccess conversationInternDataAccess, InternBL internBL, ConversationBL conversationBL)
        {
            _InternBL = internBL;
            _Conversation = conversationBL;
            _ConversationInternDataAccess = conversationInternDataAccess;
    
        }

        public List<ConversationInternResponseDTO> GetAllInternConversation(int internID)
        {
            return _ConversationInternDataAccess.GetAllInternConversation(internID);
        }


        public bool AddInternToConversation(int conversationID, int internID)
        {
            if (!_InternBL.IsInternExists(internID) || !_Conversation.IsConversationExists(conversationID))
            {
                Console.WriteLine("Yeni eklenen stajyer sohbete dahil edilemedi. stajyer/sohbet silinmiş olabilir");
                return false;
            }
            return _ConversationInternDataAccess.AddInternToConversation(conversationID, internID);
        }

        public bool DoesInternExistsInConversations(int conversationID, int internID)
        {
            return _ConversationInternDataAccess.DoesInternExistsInConversations(conversationID, internID);


        }
        public bool RemoveInternConversation(int conversationID, int internID)
        {
            if(!DoesInternExistsInConversations(conversationID, internID))
            {
                return false;
            }
            return _ConversationInternDataAccess.RemoveInternFromConversation(conversationID, internID);
        }


        public List<InterGroupListResponseDTO> GetAllConversationIntern(int conversationID)
        {
            return _ConversationInternDataAccess.GetAllConversationIntern(conversationID);
        }   

    }
}