using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Announcement
{

    [Table("Announcements")]
    public class AnnouncementEntity
    {
        [Key]
        public int AnnouncementID { get; set; }

        [Required]
        [MaxLength(200)] 
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int CreatedByUserID { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

      
        public int LastUpdatedByUserID { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public AnnouncementEntity(int announcementID, string title, string content, int createdByUserID, DateTime createdDate, int lastUpdatedByUserID, DateTime lastUpdateDate)
        {
            AnnouncementID = announcementID;
            Title = title;
            Content = content;
            CreatedByUserID = createdByUserID;
            CreatedDate = createdDate;
            LastUpdatedByUserID = lastUpdatedByUserID;
            LastUpdateDate = lastUpdateDate;
        }
}   }

