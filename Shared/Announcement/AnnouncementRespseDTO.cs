
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Shared.Announcement
{
    public class AnnouncementResponseDTO
    {

        [Required]
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

        public AnnouncementResponseDTO(int announcementID, string title, string content, DateTime createdDate, int createdByUserID)
        {
            this.AnnouncementID = announcementID;
            Title = title;
            Content = content;
            CreatedDate = createdDate;
            CreatedByUserID = createdByUserID;
        }
    }
}
