
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Shared.Announcement
{
    public class AnnouncementResponseDTO
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

        public AnnouncementResponseDTO(string title, string content, DateTime createdDate, int createdByUserID)
        {

            Title = title;
            Content = content;
            CreatedDate = createdDate;
            CreatedByUserID = createdByUserID;
        }
    }
}
