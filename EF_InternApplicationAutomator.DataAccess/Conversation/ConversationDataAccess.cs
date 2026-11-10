using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application;
using Shared.Intern;


namespace EF_InternApplicationAutomator.DataAccess.Conversation
{
    public class ConversationDataAccess
    {

        
        private readonly IAADbContext _Context;
        public ConversationDataAccess(IAADbContext context)
        {
            _Context = context;
        }
        public List<ConversationResponseDTO> GetAllConversation()
        {

            return _Context.Conversation
                .AsNoTracking()
                .Select(c => new ConversationResponseDTO(c.ConversationID,c.ConversationName,c.IsGroup))
                .ToList();
        }


        //public ApplicationResponseDTO? Find(int appID)
        //{
        //    ApplicationEntity? app = _Context.Applications.Find(appID);
        //    if (app == null)
        //    {
        //        //Log
        //        return null;
        //    }
        //    else
        //    {
        //        return new ApplicationResponseDTO(app.ApplicationID, app.PersonID, app.ReviewedByUserID, app.ReviewDate, app.ApplicationDate, app.University, app.Department, app.ClassYear, app.Status, app.Notes, app.LinkedinURL, app.GithubURL);
        //    }
        //}
        //public int AddApplication(ApplicationCreateDTO app)
        //{

        //    ApplicationEntity appEntity = new ApplicationEntity(
        //        0, app.PersonID, app.ReviewedByUserID, app.ReviewDate, app.ApplicationDate, app.University, app.Department, app.ClassYear, app.Status, app.Notes, app.LinkedinURL, app.GithubURL
        //    );

        //    EntityEntry<ApplicationEntity> pw = _Context.Applications.Add(appEntity);

        //    if (_Context.SaveChanges() > 0)
        //    {
        //        return appEntity.ApplicationID;
        //    }

        //    return -1;
        //}

        //public int UpdateStatus(int appID, ApplicationStatusUpdateDTO appResponseDTO)
        //{
        //    var app = _Context.Applications.Find(appID);

        //    if (app == null || appResponseDTO == null)
        //    {
        //        return -1;
        //    }

        //    //TODO: 2 yerine LOGİN yapmış use'ın ID'sini ver
        //    app.ReviewedByUserID = 2;
        //    app.ReviewDate = DateTime.Now.Date;
        //    app.Status = appResponseDTO.status;
        //    app.Notes = appResponseDTO.Notes;


        //    if (_Context.SaveChanges() > 0)
        //    {
        //        return appID;
        //    }

        //    return -1;
        //}

        //public bool DeleteApplication(int appID)
        //{
        //    return _Context.Applications
        //            .Where(c => c.ApplicationID == appID)
        //            .ExecuteDelete() > 0;
        //}

        //public bool IsApplicationExists(int appID)
        //{
        //    return _Context.Applications.Any(app=>app.ApplicationID==appID);
        //}

    }
}
