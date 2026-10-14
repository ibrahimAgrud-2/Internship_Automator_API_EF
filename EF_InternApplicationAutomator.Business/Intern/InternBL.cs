using EF_InternApplicationAutomator.DataAccess.Intern;
using Shared.Intern;

namespace EF_InternApplicationAutomator.Business.Intern
{
    public class InternBL
    {
        private readonly InternDataAccess _InternDataAccess;

        public InternBL(InternDataAccess internDataAccess)
        {
            _InternDataAccess = internDataAccess;
        }

        public enum enStatus { newIntern=0,separated=1};
        

        public List<InternDetailedResponseDTO> GetDetailedInternRecord()
        {
            return _InternDataAccess.GetDetailedInternRecord();
        }

        public InternResponseDTO Find(int internID)
        {
            return _InternDataAccess.Find(internID);
        }
        public InterLoginResponseDTO? FindByPasswordAndEmail(string email,string password)
        {
            return _InternDataAccess.FindByPasswordAndEmail(email,password);
        }
        public InternResponseDTO FindByPersonID(int personID)
        {
            return _InternDataAccess.FindByPersonID(personID);
        }
        public int UpdateIntern(int internID, InternCreateDTO internCreateDTO)
        {
            return _InternDataAccess.UpdateIntern(internID, internCreateDTO);
        }

        public int AddIntern(InternCreateDTO newIntern)
        {
            return _InternDataAccess.AddIntern(newIntern);
        }

        public bool DeleteIntern(int internID)
        {
            return _InternDataAccess.DeleteIntern(internID);
        }

        public bool IsInternExists(int internID)
        {
            return _InternDataAccess.IsInternExists(internID);
        }


     

    }
}