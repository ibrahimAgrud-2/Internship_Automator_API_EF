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

        public List<ProjectListResponseDTO> GetAllProjects()
        {
            return _ProjectDataAccess.GetAllProjects();
        }

        public ProjectResponseDTO? Find(int projectID)
        {
            return _ProjectDataAccess.Find(projectID);
        }

        public int AddNewProject(ProjectCreateDTO newProject)
        {
            return _ProjectDataAccess.AddNewProject(newProject);
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
