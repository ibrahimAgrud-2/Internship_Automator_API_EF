
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Shared.Announcement
{
    public class AnnouncementCreateDTO
    {

  

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int CreatedByUserID { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public AnnouncementCreateDTO( string title, string content, int createdByUserID, DateTime createdDate)
        {

            Title = title;
            Content = content;
            CreatedByUserID = createdByUserID;
            CreatedDate = createdDate;
 
        }
    }
}
