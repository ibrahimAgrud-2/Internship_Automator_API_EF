
using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.ConversationIntern;
using EF_InternApplicationAutomator.DataAccess.Message;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared;
using Shared.Application;
using Shared.Intern;
using Shared.Message;
using Shared.Conversation;
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
           ConversationInternDataAccess _ConversationInternDataAccess;
            public ConversationBL(ConversationDataAccess conversationDataAccess, MessagesDataAccess messagesDataAccess, ConversationInternDataAccess conversationInternDataAccess)
        {
            _ConversationDataAccess = conversationDataAccess;
            _MessagesDataAccess = messagesDataAccess;
            _ConversationInternDataAccess = conversationInternDataAccess;
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

        public int AddMessage(MessageCreateDTO message,int personID,bool isStaff)
        {
            return _MessagesDataAccess.AddMessage(message, personID,isStaff);
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
           
            return _ConversationDataAccess.DoesInternExistsInConversations(internID,conversationID);
        }
   
        public int AddNewConversation(ConversationCreateDTO conversation)
        {
            return _ConversationDataAccess.AddNewConversation(conversation);
        }


        public int DoesInternHasAPrivateChat(int internID)
        {
            return _ConversationInternDataAccess.DoesInternHasAPrivateChat(internID);
        }

        //eğer bir stajyer sistemde yoksa, mesela olmayan stajyerin ID'si verilmişse bu çalışmayı etkilemicek
        //Öyleki tüm ID'ler yanlış olsa bile hiç üye olmadan bir grup oluşacak. Personel isterse sonradan üye 
        //ekleyebilir. Olmayan intern eklenmicek
        public int AddNewGroupConversation(GroupConversationCreateDTO conversationCreateDTO)
        {
            int groupConversationID = AddNewConversation(new ConversationCreateDTO(conversationCreateDTO.ConversationName,true));
            if(groupConversationID<1)
            {
                return -1;
            } else
            {
                foreach (var internID in conversationCreateDTO.InterIDs)
                {
                  
                    _ConversationInternDataAccess.AddInternToConversation(groupConversationID, internID);
                }
            }
            return groupConversationID;
        }
    }
}