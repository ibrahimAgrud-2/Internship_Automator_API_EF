using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using Shared.OngoingProjects;

namespace EF_InternApplicationAutomator.Business.OngoingProjects
{
    public class OngoingProjectsBL
    {
        private readonly OngoingProjectDataAccess _OngoingProjectDataAccess;

        public OngoingProjectsBL(OngoingProjectDataAccess ongoingProjectDataAccess)
        {
            _OngoingProjectDataAccess = ongoingProjectDataAccess;
        }

        public List<OngoingInternProjectResponse> GetInternOngoingProjects(int internID)
        {
            return _OngoingProjectDataAccess.GetInternOngoingProjects(internID);
        }

        public OngoingProjectResponseDTO? Find(int ongoingProjectID)
        {
            return _OngoingProjectDataAccess.Find(ongoingProjectID);
        }

        public int UpdateOngoingProject(
            int ongoingProjectID,
            OngoingProjectCreateDTO ongoingProjectCreateDTO)
        {
            return _OngoingProjectDataAccess.UpdateOngoingProject(
                ongoingProjectID,
                ongoingProjectCreateDTO);
        }

        public int AddOngoingProject(OngoingProjectCreateDTO newOngoingProject)
        {
            return _OngoingProjectDataAccess.AddOngoingProject(newOngoingProject);
        }

        public bool DeleteOngoingProject(int ongoingProjectID)
        {
            return _OngoingProjectDataAccess.DeleteOngoingProject(ongoingProjectID);
        }

        public bool IsOngoingProjectExists(int ongoingProjectID)
        {
            return _OngoingProjectDataAccess.IsOngoingProjectExists(ongoingProjectID);
        }
    }
}
