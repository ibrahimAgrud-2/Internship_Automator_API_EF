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



        public List<MessageResponseDTO> GetAllMessagesForConversation(int conversationID,int messageCount=50)
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
      




        //public List<ConversationResponseDTO> GetAll()
        //{
        //    return _ConversationDataAccess.GetAll();
        //}
        //public enum enStatus { newIntern=0,separated=1};


        //public List<InternDetailedResponseDTO> GetDetailedInternRecord()
        //{
        //    return _MessageDataAccess.GetDetailedInternRecord();
        //}

        //public InternCardResponseDTO Find(int internID)
        //{
        //    return _MessageDataAccess.Find(internID);
        //}
        //public InterLoginResponseDTO? FindByPasswordAndEmail(string email,string password)
        //{
        //    return _MessageDataAccess.FindByPasswordAndEmail(email,password);
        //}
        //public InternResponseDTO FindByPersonID(int personID)
        //{
        //    return _MessageDataAccess.FindByPersonID(personID);
        //}
        //public int UpdateIntern(int internID, InternCreateDTO internCreateDTO)
        //{
        //    return _MessageDataAccess.UpdateIntern(internID, internCreateDTO);
        //}

        //public int AddIntern(InternCreateDTO newIntern)
        //{
        //    return _MessageDataAccess.AddIntern(newIntern);
        //}

        //public bool DeleteIntern(int internID)
        //{
        //    return _MessageDataAccess.DeleteIntern(internID);
        //}

        //public bool IsInternExists(int internID)
        //{
        //    return _MessageDataAccess.IsInternExists(internID);
        //}


        //public bool IsInternActive(int internID)
        //{
        //    return _MessageDataAccess.IsInternActive(internID); 
        //}

        //public bool IsProjectAssignedToIntern(int projectID,int internID)
        //{
        //    return _OngoingProjectDataAccess.IsProjectAssignedToIntern(projectID, internID);
        //}

    }
}