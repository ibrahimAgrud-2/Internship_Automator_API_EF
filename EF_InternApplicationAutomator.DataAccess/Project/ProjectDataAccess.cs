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
            List<ProjectEntity> projects = _Context.Projects.ToList();
            List<ProjectResponseDTO> projectDTOs = new List<ProjectResponseDTO>();

            foreach (var project in projects)
            {
                projectDTOs.Add(new ProjectResponseDTO(
                    project.ProjectID,
                    project.CreatedByUserID,
                    project.Title,project.Description,
                    project.Technologies));
            }

            return projectDTOs;
        }

        public ProjectResponseDTO? Find(int projectID)
        {
            ProjectEntity? project = _Context.Projects.Find(projectID);
            if (project == null)
            {
                return null;
            }


            return new ProjectResponseDTO(
                project.ProjectID,
                project.CreatedByUserID,
                project.Title,project.Description,
                project.Technologies);
        }

        public int AddProject(ProjectCreateDTO project)
        {
            ProjectEntity projectEntity = new ProjectEntity(
                0,
                project.CreatedByUserID,
                project.Title,project.Description,
                project.Technologies);
            _Context.Projects.Add(projectEntity);

            if (_Context.SaveChanges() > 0)
            {
                return projectEntity.ProjectID;
            }

            return -1;
        }

        public int UpdateProject(int projectID, ProjectCreateDTO projectDTO)
        {
            ProjectEntity? project = _Context.Projects.Find(projectID);
            if (project == null || projectDTO == null)
            {
                return -1;
            }

            project.CreatedByUserID = projectDTO.CreatedByUserID;
            project.Title = projectDTO.Title;
            project.Technologies = projectDTO.Technologies;

            if (_Context.SaveChanges() > 0)
            {
                return projectID;
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
