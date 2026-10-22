using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.Messages;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using EF_InternApplicationAutomator.DataAccess.User;
using Shared.Conversations;
using Shared.Intern;
using Shared.Message;

namespace EF_InternApplicationAutomator.Business.conversation
{
    public class ConversationBL
    {
        private readonly ConversationDataAccess _ConversationDataAccess;
        private readonly UserDataAccess _User;
        private readonly MessageDataAccess _MessageData;


        public ConversationBL(ConversationDataAccess conversationDataAccess, UserDataAccess user, MessageDataAccess message)
        {
            _ConversationDataAccess = conversationDataAccess;
            _User = user;
            _MessageData = message;
        }



      
        public List<vw_MessagesWithContentDTO> GetAllMessagesForConversation(int conversationID, int messageCount = 50)
        {
            return _ConversationDataAccess.GetAllMessagesForConversation(conversationID, messageCount);
        }

        public bool IsConversationExists(int conversationID)
        {
          return   _ConversationDataAccess.IsConversationExists(conversationID);
        }

        //public StaffAddMessageDTO? InternAddMessage(StaffAddMessageDTO newMessage)
        //{
        //    int personID = _User.Find(newMessage.UserID).PersonID;

        //    return _ConversationDataAccess.AddMessage(new MessageCreateDTO(personID, newMessage.ConversationID, newMessage.Content, true));
        //}
        public bool StaffAddMessage(StaffAddMessageDTO newMessage)
        {
            int personID = _User.Find(newMessage.UserID).PersonID;

            return _MessageData.AddMessage(new MessageCreateDTO(personID,newMessage.ConversationID,newMessage.Content,true,newMessage.ReferencedContentID,newMessage.ReferencedContentType));
        }

        public bool AddInternToConversation(int internID, int conversationID)
        {
            //TODO: add yaparken intern'in zaten sobhette olup olmadığu veya
            //intern'İn ID'si sistemde olup  olmaduı - isExist kontroller yap
            //varsa ekleme.
           return  _ConversationDataAccess.AddInternToConversation(internID, conversationID);
        }

        public void AddInternsToConversation(int[] internIDs, int conversationID)
        {
            //TODO: add yaparken intern'in zaten sobhette olup olmadığu veya
            //intern'İn ID'si sistemde olup  olmaduı - isExist kontroller yap
            //varsa ekleme. 
            for (int i = 0; i < internIDs.Length; i++)
            {
                _ConversationDataAccess.AddInternToConversation(internIDs[i], conversationID);
            }
        }
        public ConversationResponseDTO AddNewConversation(ConversationCreateDTO conversation)
        {
            
             ConversationResponseDTO cr=_ConversationDataAccess.AddNewConversation(conversation);

            AddInternsToConversation(conversation.InternIDs, cr.ConversationID);
            return cr;
        }
    

        public ConversationResponseDTO? Find(int ID)
        {
            return _ConversationDataAccess.Find(ID);
        }
        //public ConversationResponseDTO? FindByInternID(int ID)
        //{
        //    return _ConversationDataAccess.FindByInternID(ID);
        //}
        //public bool IsInternHasPrivateConversation(int interID)
        //{
        //    return _ConversationDataAccess.IsInternHasPrivateConversation(interID);
        //}

        public List<ConversationResponseDTO> GetAllConversation()
        {
            return _ConversationDataAccess.GetAllConversation();
        }


    }
}