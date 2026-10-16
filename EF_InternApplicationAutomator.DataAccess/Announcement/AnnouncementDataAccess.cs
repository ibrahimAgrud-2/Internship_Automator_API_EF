using MailKit.Search;
using Microsoft.EntityFrameworkCore;
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
                announcementDTOs.Add(new AnnouncementResponseDTO(a.Title,a.Content,a.CreatedDate,a.CreatedByUserID));
            }
            return announcementDTOs;
        }
    }
}
