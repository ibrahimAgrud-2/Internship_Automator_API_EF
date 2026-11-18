
using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.Message;
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
       MessagesDataAccess _MessagesDataAccess;
        public ConversationBL(ConversationDataAccess conversationDataAccess, MessagesDataAccess messagesDataAccess)
        {
            _ConversationDataAccess = conversationDataAccess;
            _MessagesDataAccess = messagesDataAccess;
        }

        public List<ConversationResponseDTO>GetAllConversation()
        {
            return _ConversationDataAccess.GetAllConversation();
        }
        public List<MessageDetailedResponseDTO> GetAllMessages(int conversationID)
        {
            return _ConversationDataAccess.GetAllMessages(conversationID);
        }

        public ConversationResponseDTO? Find(int ID)
        {
            return _ConversationDataAccess.Find(ID);
        }

        public int AddMessage(MessageCreateDTO message,bool isStaff)
        {
            return _MessagesDataAccess.AddMessage(message, isStaff);
        }



        public bool IsConversationExists( int conversationID)
        {

            return _ConversationDataAccess.IsConversationExists(conversationID);
        }

        /// <summary>
        /// Bir person verilen sohbete mesaj ekleyebilir mi ekleyemez mi kontrolü yapar
        /// Personel için şu anlık bu kontrol gererk yok çünkü onlar tüm chatlara erişebilir
        /// Ana Bu özellikse intern için lazım. Bir itnern sadece dahil olduğu chatlara mesaj atabilir
        /// </summary>
        /// <returns></returns>
        public bool IsInternExistsInConversations(int internID,int conversationID)
        {
           
            return _ConversationDataAccess.IsInternExistsInConversations(internID,conversationID);
        }
    }
}