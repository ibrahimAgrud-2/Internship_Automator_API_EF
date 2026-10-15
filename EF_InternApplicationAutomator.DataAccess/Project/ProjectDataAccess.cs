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



        public List<ProjectListResponseDTO> GetAllProjects()
        {
            List<ProjectEntity> projects = _Context.Projects.Include(p => p.User).ToList();
            List<ProjectListResponseDTO> projectDTOs = new List<ProjectListResponseDTO>();

            foreach (var project in projects)
            {
                projectDTOs.Add(new ProjectListResponseDTO(
                    project.ProjectID,
                    project.CreatedByUserID, project.User.UserName,
                    project.Title,project.Description,
                    project.Technologies));
            }

          

            return projectDTOs;
        }
    
        public ProjectResponseDTO? Find(int projectID)
        {
            ProjectEntity pe= _Context.Projects.Find(projectID);

            return (pe==null)?null:new ProjectResponseDTO(pe.ProjectID,pe.Title,pe.Description,pe.Technologies);
        }
        
        
        public int AddNewProject(ProjectCreateDTO project)
        {
            ProjectEntity projectEntity = new ProjectEntity(
                0,
                project.CreatedByUserID,
                project.Title, project.Description,
                project.Technologies);
            _Context.Projects.Add(projectEntity);

            if (_Context.SaveChanges() > 0)
            {
                return projectEntity.ProjectID;
            }

            return -1;
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
