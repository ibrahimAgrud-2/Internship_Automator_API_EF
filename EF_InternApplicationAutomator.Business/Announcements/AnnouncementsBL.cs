using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.Announcement;
using Shared;
using Shared.Announcement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.Business.Announcements
{
    public class AnnouncementsBL
    {
        AnnouncementDataAccess _AnnouncementData;

        //PersonBL ile DAL'ı bağlıyor.
        public AnnouncementsBL(AnnouncementDataAccess announcementData)
        {
            _AnnouncementData = announcementData;

        }


        public List<AnnouncementResponseDTO> GetAll()
        {
            return _AnnouncementData.GetAll();
        }


    }
}
