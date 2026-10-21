using EF_InternApplicationAutomator.DataAccess.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Application;
using Shared.OngoingProjects;
using Shared.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.OngoingProjects
{
    public class OngoingProjectDataAccess
    {
        private readonly IAADbContext _Context;

        public OngoingProjectDataAccess(IAADbContext context)
        {
            _Context = context;
        }

        public List<OngoingInternProjectResponseDTO> GetInternOngoingProjects(int internID)
        {

            /*
             
                 return _Context.Applications
                .AsNoTracking()
                .Select(a => new ApplicationListDTO(new Shared.PersonCreateSDTO(a.Person.FirstName,a.Person.LastName,a.Person.Email,a.Person.Phone,a.Person.Address,a.Person.ImagePath),a.ApplicationID, a.Status,a.ApplicationDate, a.University,a.Department,a.ClassYear,a.Notes,a.LinkedinURL,a.GithubURL
                ))
                .ToList();
             */
            List<OngoingProjectEntity> ongoingProjectEntities = _Context.OngoingProjects.Include(t => t.Project).Where(t => t.InternID==internID).ToList();
            List<OngoingInternProjectResponseDTO> taskDTOs = new List<OngoingInternProjectResponseDTO>();

            foreach (var op in ongoingProjectEntities)
            {
                taskDTOs.Add(new OngoingInternProjectResponseDTO (op.OngoingProjectID,op.Project.Title,op.Project.Technologies,op.ProjectID,op.StartDate,op.DueDate));
            }

            return taskDTOs;
        }
        
        public int GetAssignedInternCountForProject(int projectID)
        {
            return _Context.OngoingProjects.Where(p=>p.ProjectID==projectID).Count();
        }
        public int GetInternOngoingProjectsCount(int internID)
        {
            return _Context.OngoingProjects.Include(t => t.Project).Where(t => t.InternID == internID).Count();
                
        }
        public OngoingProjectResponseDTO? Find(int ongoingProjectID)
        {
            OngoingProjectEntity? ongoingProject = _Context.OngoingProjects.Find(ongoingProjectID);
            return ongoingProject == null ? null : ToResponseDTO(ongoingProject);
        }

        public int AddOngoingProject(OngoingProjectCreateDTO ongoingProject)
        {
            OngoingProjectEntity entity = new OngoingProjectEntity(
                0,
                ongoingProject.InternID,
                ongoingProject.ProjectID,
                DateTime.Now.Date,
                ongoingProject.DueDate);
            _Context.OngoingProjects.Add(entity);

            if (_Context.SaveChanges() > 0)
            {
                return entity.OngoingProjectID;
            }

            return -1;
        }


        public int UpdateOngoingProject(int ongoingProjectID, OngoingProjectCreateDTO ongoingProjectDTO)
        {
            OngoingProjectEntity? ongoingProject = _Context.OngoingProjects.Find(ongoingProjectID);
            if (ongoingProject == null || ongoingProjectDTO == null)
            {
                return -1;
            }

            ongoingProject.InternID = ongoingProjectDTO.InternID;
            ongoingProject.ProjectID = ongoingProjectDTO.ProjectID;
            ongoingProject.StartDate = DateTime.Now.Date;
            ongoingProject.DueDate = ongoingProjectDTO.DueDate;

            if (_Context.SaveChanges() > 0)
            {
                return ongoingProjectID;
            }

            return -1;
        }

        public bool DeleteOngoingProject(int ongoingProjectID)
        {
            return _Context.OngoingProjects
                .Where(p => p.OngoingProjectID == ongoingProjectID)
                .ExecuteDelete() > 0;
        }

        public bool IsOngoingProjectExists(int ongoingProjectID)
        {
            return _Context.OngoingProjects.Any(
                p => p.OngoingProjectID == ongoingProjectID);
        }


        public bool IsProjectAssignedToIntern(int projectID,int internID)
        {
            return _Context.OngoingProjects.Any(
                p => p.ProjectID == projectID&&p.InternID==internID);
        }

        private static OngoingProjectResponseDTO ToResponseDTO(OngoingProjectEntity ongoingProject)
        {
            return new OngoingProjectResponseDTO(
                ongoingProject.OngoingProjectID,
                ongoingProject.InternID,
                ongoingProject.ProjectID,
                ongoingProject.StartDate,
                ongoingProject.DueDate);
        }
    }
}
