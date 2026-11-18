using EF_InternApplicationAutomator.DataAccess.Data;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared;
using Shared.Announcement;
using Shared.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Announcement
{
    public class AnnouncementDataAccess
    {
        private readonly IAADbContext _Context;

        public AnnouncementDataAccess(IAADbContext context)
        {
            _Context = context;
        }
        public List<AnnouncementResponseDTO> GetAll(int topNewest)
        {
            List<AnnouncementEntity> announcements = _Context.Announcements.OrderByDescending(m => m.CreatedDate).Take(topNewest).ToList();

            List<AnnouncementResponseDTO> announcementDTOs = new List<AnnouncementResponseDTO>();



            //mapping
            foreach (var a in announcements)
            {
                announcementDTOs.Add(new AnnouncementResponseDTO(a.AnnouncementID,a.Title,a.Content,a.CreatedDate,a.CreatedByUserID));
            }
            return announcementDTOs;
        }

        public AnnouncementResponseDTO Find(int ID)
        {
            AnnouncementEntity an= _Context.Announcements.Find(ID);
            return (an == null) ? null :new AnnouncementResponseDTO(an.AnnouncementID,an.Title,an.Content,an.CreatedDate,an.CreatedByUserID);
        }

        public int AddAnnouncements(AnnouncementCreateDTO aDTO,int createdByUserID)
        {
            AnnouncementEntity announcement = new AnnouncementEntity(0,aDTO.Title,aDTO.Content,createdByUserID,DateTime.Now,createdByUserID,DateTime.Now);
            EntityEntry<AnnouncementEntity> pw = _Context.Announcements.Add(announcement);

            if (_Context.SaveChanges() > 0)
            {
                return announcement.AnnouncementID;
            }

            return -1;
        }

        public bool DeleteAnnouncement(int ID)
        {
            return _Context.Announcements
                    .Where(p => p.AnnouncementID == ID)
                    .ExecuteDelete() > 0;
        }

        public bool UpdateAnnouncement(int announcementID, int updatedByUserID,AnnouncementUpdateDTO update)
        {
            var app = _Context.Announcements.Find(announcementID);

            if (app == null || update == null)
            {
                return false;
            }

           
            app.Title = update.Title;
            app.Content = update.Content;
            app.LastUpdateDate = DateTime.Now;
            app.LastUpdatedByUserID = updatedByUserID;

            if (_Context.SaveChanges() > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsAnnouncementExists(int ID)
        {
            return _Context.Announcements.Any(a=>a.AnnouncementID==ID);
        }

    }
}
