using EF_InternApplicationAutomator.Business.Conversation;
using EF_InternApplicationAutomator.Business.ConversationIntern;
using EF_InternApplicationAutomator.DataAccess.Conversation;
using EF_InternApplicationAutomator.DataAccess.ConversationIntern;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using Shared.Conversation;
using Shared.Intern;

namespace EF_InternApplicationAutomator.Business.Intern
{
    public class InternBL
    {
        private readonly InternDataAccess _InternDataAccess;
        private readonly OngoingProjectDataAccess _OngoingProjectDataAccess;
        private readonly ConversationInternDataAccess _ConversationInternDA;
        private readonly  ConversationDataAccess _ConversationDA;

        public InternBL(InternDataAccess internDataAccess, OngoingProjectDataAccess ongoingProjectDataAccess, ConversationInternDataAccess conversationInternDA, ConversationDataAccess conversationDA)
        {
            _InternDataAccess = internDataAccess;
            _OngoingProjectDataAccess = ongoingProjectDataAccess;
            _ConversationInternDA = conversationInternDA;
            _ConversationDA = conversationDA;

        }

        public enum enStatus { newIntern = 1, separated = 0 };


        public List<InternDetailedResponseDTO> GetDetailedInternRecord()
        {
            return _InternDataAccess.GetDetailedInternRecord();
        }

        public InternCardResponseDTO Find(int internID)
        {
            return _InternDataAccess.Find(internID);
        }

        public InternResponseDTO FindByPersonID(int personID)
        {
            return _InternDataAccess.FindByPersonID(personID);
        }
        public bool UpdateIntern(int internID, InternUpdateDTO internCreateDTO)
        {
            return _InternDataAccess.UpdateIntern(internID, internCreateDTO);
        }

        public int AddIntern(InternCreateDTO newIntern)
        {
            newIntern.CreatedDate = DateTime.Now;
            int insertedInternID= _InternDataAccess.AddIntern(newIntern);
            InternCardResponseDTO insertedIntern = _InternDataAccess.Find(insertedInternID);
            if(insertedIntern==null||insertedInternID > 0)
            {
                //after adding intern, we should include inter to the interns group chat and a private chat with staf

                //1)Add to general group chat
                _ConversationInternDA.AddInternToConversation(9, insertedInternID);

                //2)intern için personlle bir sohbet oluşturup oraya eklicem
                ConversationCreateDTO conversation = new ConversationCreateDTO(insertedIntern.FirstName,false);
                int insertedConversationID = _ConversationDA.AddNewConversation(conversation);
                if (insertedConversationID>0)
                {
                    _ConversationInternDA.AddInternToConversation(insertedConversationID, insertedInternID);
                    return insertedInternID;
                }
                else
                {
                    return - 1;
                }


            }
            return -1;
        }

        public bool DeleteIntern(int internID)
        {
            return _InternDataAccess.DeleteIntern(internID);
        }

        public bool IsInternExists(int internID)
        {
            return _InternDataAccess.IsInternExists(internID);
        }


        public bool IsInternActive(int internID)
        {
            return _InternDataAccess.IsInternActive(internID);
        }

        public bool IsProjectAssignedToIntern(int projectID, int internID)
        {
            return _OngoingProjectDataAccess.IsProjectAssignedToIntern(projectID, internID);
        }

        public int GetInternIDByPersonID(int personID)
        {
            return _InternDataAccess.GetInternIDByPersonID(personID);
        }

        public bool SeparateIntern(int internID)
        {
            return _InternDataAccess.SeparateIntern(internID);
        }

    }
}