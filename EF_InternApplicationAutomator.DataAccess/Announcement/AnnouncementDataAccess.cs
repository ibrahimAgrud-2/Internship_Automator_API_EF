using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared;
using Shared.Announcement;
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
        public List<AnnouncementResponseDTO> GetAll()
        {
            List<AnnouncementEntity> announcements = _Context.Announcements.OrderBy(a => a.CreatedDate).ToList();

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

        public int AddAnnouncements(AnnouncementCreateDTO aDTO)
        {
            AnnouncementEntity announcement = new AnnouncementEntity(0,aDTO.Title,aDTO.Content,aDTO.CreatedByUserID,aDTO.CreatedDate,aDTO.CreatedByUserID,DateTime.Now.Date);
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


    }
}
