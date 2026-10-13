using Microsoft.EntityFrameworkCore;
using Shared.Application;
using Shared.OngoingProjects;

namespace EF_InternApplicationAutomator.DataAccess.OngoingProjects
{
    public class OngoingProjectDataAccess
    {
        private readonly IAADbContext _Context;

        public OngoingProjectDataAccess(IAADbContext context)
        {
            _Context = context;
        }

        public List<OngoingInternProjectResponse> GetInternOngoingProjects(int internID)
        {

            /*
             
                 return _Context.Applications
                .AsNoTracking()
                .Select(a => new ApplicationListDTO(new Shared.PersonCreateSDTO(a.Person.FirstName,a.Person.LastName,a.Person.Email,a.Person.Phone,a.Person.Address,a.Person.ImagePath),a.ApplicationID, a.Status,a.ApplicationDate, a.University,a.Department,a.ClassYear,a.Notes,a.LinkedinURL,a.GithubURL
                ))
                .ToList();
             */

            return _Context.OngoingProjects.Where(p => p.InternID == internID).Select(op => new OngoingInternProjectResponse(op.OngoingProjectID, op.Project.Title, op.Project.Technologies, op.ProjectID, op.StartDate, op.DueDate)).ToList();
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
                ongoingProject.StartDate,
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
            ongoingProject.StartDate = ongoingProjectDTO.StartDate;
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
