using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using Shared.Conversations;
using Shared.Intern;

namespace EF_InternApplicationAutomator.Business.conversation
{
    public class ConversationBL
    {
        private readonly ConversationDataAccess _ConversationDataAccess;


        public ConversationBL(ConversationDataAccess conversationDataAccess)
        {
            _ConversationDataAccess = conversationDataAccess;
        }

        public List<ConversationResponseDTO> GetAll()
        {
            return _ConversationDataAccess.GetAll();
        }

        //public enum enStatus { newIntern=0,separated=1};
        

        //public List<InternDetailedResponseDTO> GetDetailedInternRecord()
        //{
        //    return _ConversationDataAccess.GetDetailedInternRecord();
        //}

        //public InternCardResponseDTO Find(int internID)
        //{
        //    return _ConversationDataAccess.Find(internID);
        //}
        //public InterLoginResponseDTO? FindByPasswordAndEmail(string email,string password)
        //{
        //    return _ConversationDataAccess.FindByPasswordAndEmail(email,password);
        //}
        //public InternResponseDTO FindByPersonID(int personID)
        //{
        //    return _ConversationDataAccess.FindByPersonID(personID);
        //}
        //public int UpdateIntern(int internID, InternCreateDTO internCreateDTO)
        //{
        //    return _ConversationDataAccess.UpdateIntern(internID, internCreateDTO);
        //}

        //public int AddIntern(InternCreateDTO newIntern)
        //{
        //    return _ConversationDataAccess.AddIntern(newIntern);
        //}

        //public bool DeleteIntern(int internID)
        //{
        //    return _ConversationDataAccess.DeleteIntern(internID);
        //}

        //public bool IsInternExists(int internID)
        //{
        //    return _ConversationDataAccess.IsInternExists(internID);
        //}


        //public bool IsInternActive(int internID)
        //{
        //    return _ConversationDataAccess.IsInternActive(internID); 
        //}

        //public bool IsProjectAssignedToIntern(int projectID,int internID)
        //{
        //    return _OngoingProjectDataAccess.IsProjectAssignedToIntern(projectID, internID);
        //}
    
    }
}