using Microsoft.EntityFrameworkCore;
using Shared.Project;

namespace EF_InternApplicationAutomator.DataAccess.Project
{
    public class ProjectDataAccess
    {
        private readonly IAADbContext _Context;

        public ProjectDataAccess(IAADbContext context)
        {
            _Context = context;
        }

        public List<ProjectResponseDTO> GetAllProjects()
        {
            List<ProjectEntity> projects = _Context.Projects.Include(p => p.User).ToList();
            List<ProjectResponseDTO> projectDTOs = new List<ProjectResponseDTO>();

            foreach (var project in projects)
            {
                projectDTOs.Add(new ProjectResponseDTO(
                    project.ProjectID,
                    project.CreatedByUserID, project.User.UserName,
                    project.Title,project.Description,
                    project.Technologies));
            }

          

            return projectDTOs;
        }

      
        public bool DeleteProject(int projectID)
        {
            return _Context.Projects
                .Where(project => project.ProjectID == projectID)
                .ExecuteDelete() > 0;
        }

        public bool IsProjectExists(int projectID)
        {
            return _Context.Projects.Any(project => project.ProjectID == projectID);
        }
    }
}
