using EF_InternApplicationAutomator.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application;


namespace EF_InternApplicationAutomator.DataAccess.Application
{
    public class ApplicationDataAccess
    {

        
        private readonly IAADbContext _Context;
        public ApplicationDataAccess(IAADbContext context)
        {
            _Context = context;
        }


        public List<ApplicationDetailedListDTO> GetAllApplication()
        {

            return _Context.Applications
              .AsNoTracking()
              .Select(a => new ApplicationDetailedListDTO(a.ApplicationID, a.Status, a.ApplicationDate, a.University, a.Department, a.ClassYear, a.Notes, a.LinkedInUrl, a.GitHubUrl))
              .ToList();
        }

        public ApplicationResponseDTO? Find(int appID)
        {
            ApplicationEntity? app = _Context.Applications.Find(appID);
            if (app == null)
            {
                //Log
                return null;
            }
            else
            {
                return new ApplicationResponseDTO(app.ApplicationID, app.PersonID, app.ReviewedByUserID, app.ReviewDate, app.ApplicationDate, app.University, app.Department, app.ClassYear, app.Status);
            }
        }
        public int AddApplication(ApplicationCreateDTO app)
        {

            ApplicationEntity appEntity = new ApplicationEntity(
                0,app.PersonID, app.ReviewedByUserID, app.ReviewDate, app.ApplicationDate, app.University, app.Department, app.ClassYear, app.Status,app.Notes,app.LinkedinURL,app.GithubURL
            );

            EntityEntry<ApplicationEntity> pw = _Context.Applications.Add(appEntity);

            if (_Context.SaveChanges() > 0)
            {
                return appEntity.ApplicationID;
            }

            return -1;
        }

        public bool UpdateStatus(int appID,byte status,int reviewedBuUserID, string? note)
        {
            var app = _Context.Applications.Find(appID);

            if (app == null||status>3)
            {
                return false;
            }

           
            app.ReviewedByUserID = reviewedBuUserID;
            app.ReviewDate = DateTime.Now.Date;
            app.Status = status;
            app.Notes = note;

            if (_Context.SaveChanges() > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteApplication(int appID)
        {
            return _Context.Applications
                    .Where(a => a.ApplicationID == appID)
                    .ExecuteDelete() > 0;
        }

        public bool IsApplicationExists(int appID)
        {
            return _Context.Applications.Any(app=>app.ApplicationID==appID);
        }

    }
}
