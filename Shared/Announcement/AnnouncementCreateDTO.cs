
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


        public AnnouncementCreateDTO( string title, string content)
        {

            Title = title;
            Content = content;
   
        }
    }
}
