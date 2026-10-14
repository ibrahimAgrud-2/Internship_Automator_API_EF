using EF_InternApplicationAutomator.DataAccess.Project;
using Shared.Project;

namespace EF_InternApplicationAutomator.Business.Project
{
    public class ProjectsBL
    {
        private readonly ProjectDataAccess _ProjectDataAccess;

        public ProjectsBL(ProjectDataAccess projectDataAccess)
        {
            _ProjectDataAccess = projectDataAccess;
        }

        public List<ProjectResponseDTO> GetAllProjects()
        {
            return _ProjectDataAccess.GetAllProjects();
        }

 
        public bool DeleteProject(int projectID)
        {
            return _ProjectDataAccess.DeleteProject(projectID);
        }

        public bool IsProjectExists(int projectID)
        {
            return _ProjectDataAccess.IsProjectExists(projectID);
        }
    }
}
