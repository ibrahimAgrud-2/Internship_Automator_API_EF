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

        public List<AnnouncementResponseDTO> GetAll(int topNewest=15)
        {
            return _AnnouncementData.GetAll(topNewest);
        }

        public int AddAnnouncement(AnnouncementCreateDTO announcementCreateDTO, int CreatedByuserID)
        {
            return _AnnouncementData.AddAnnouncements(announcementCreateDTO, CreatedByuserID);
        }
        public bool UpdateAnnouncement(int ID,int updatedByUserID,AnnouncementUpdateDTO update)
        {
            return _AnnouncementData.UpdateAnnouncement(ID,updatedByUserID,update);
        }

        public AnnouncementResponseDTO Find(int ID)
        {
            return _AnnouncementData.Find(ID);
        }
      
        public bool DeleteAnnouncement(int ID)
        {
            return _AnnouncementData.DeleteAnnouncement(ID);
        }

        public bool IsAnnouncementExists(int ID)
        {
            return _AnnouncementData.IsAnnouncementExists(ID);
        }

    }
}
