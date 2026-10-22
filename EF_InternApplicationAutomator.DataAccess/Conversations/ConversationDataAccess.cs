

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

        public List<ConversationResponseDTO> GetAllConversation()
        {
            List<ConversationEntity> ce = _Context.Conversations.ToList();

            List<ConversationResponseDTO> conversationResponseDTOs = new List<ConversationResponseDTO>();



            foreach (var item in ce)
            {
                conversationResponseDTOs.Add(new ConversationResponseDTO(item.ConversationID,item.InternID,item.ConversationName,item.IsGroup));
            }
            return conversationResponseDTOs;
        }


        public bool IsConversationExists(int conversationID)
        {
            return _Context.Conversations.Any(c=>c.ConversationID==conversationID);
        }


        public ConversationResponseDTO AddNewConversation(ConversationCreateDTO newConversation)
        {
            ConversationEntity conversationEntity;

            if (newConversation.IsGroup)
            {
                conversationEntity =  new ConversationEntity(0, null, newConversation.ConversationName, newConversation.IsGroup);
            }
            else
            {
                conversationEntity = new ConversationEntity(0, newConversation.InternID, newConversation.ConversationName, newConversation.IsGroup);
            }


                EntityEntry<ConversationEntity> CE = _Context.Conversations.Add(conversationEntity);

            if (_Context.SaveChanges() > 0)
            {
                return new ConversationResponseDTO(conversationEntity.ConversationID, conversationEntity.InternID, conversationEntity.ConversationName, conversationEntity.IsGroup);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// BU fonk. İntern'in private chat için zaten sohbet'i olup olmadıpını kontrol eder
        /// sistemde sohbet iki yönlü personl - intern arasında. İntern veya personle yeni bir özel sohbet
        /// Oluşturmak istediklerinde, sohbet edilecek intern için daha önceden sohbet'in oldup olmadı
        /// kontrol edilir
        /// Bu kontrol sadede private sohbet'ler için edilir. Çünkü bir stajyer birden fazla grup'ta olabileceği için
        /// önceden var olup olmadığı bu fonksiyonda kontrol edilmez.
        /// </summary>
        /// <param name="interID"></param>
        /// <returns></returns>
        public bool IsInternHasPrivateConversation(int interID)
        {
            return _Context.Conversations.Any(c=>c.InternID== interID && !c.IsGroup);
        }

         /// <summary>
                ///Buda yukardaki fonkiyon gibi ama bu fonkisyon intern'in bir grup sohbetinde ekli olupolmadığını 
                ///bulurbir gruba stajyer eklerken eğer o stajyer zaten grupta ise ekleme olmaz.
            /// </summary>
            /// <param name="interID"></param>
            /// <param name="conversationID"></param>
            /// <returns></returns>
        public bool IsInternHasInvolvedToGroupConversation(int interID,int conversationID)
        {
            return _Context.Conversations.Any(c => c.InternID == interID && c.IsGroup&&c.ConversationID== conversationID);
        }
        public ConversationResponseDTO? Find(int  ID)
        {
            ConversationEntity ce = _Context.Conversations.Find(ID);
            return (ce == null) ? null : new ConversationResponseDTO(ce.ConversationID,ce.InternID,ce.ConversationName,ce.IsGroup);
          
        }

        public ConversationResponseDTO? FindByInternID(int ID)
        {
            ConversationEntity ce = _Context.Conversations.FirstOrDefault(c => c.InternID == ID);
            return (ce == null) ? null : new ConversationResponseDTO(ce.ConversationID, ce.InternID, ce.ConversationName, ce.IsGroup);

        }

    }
}