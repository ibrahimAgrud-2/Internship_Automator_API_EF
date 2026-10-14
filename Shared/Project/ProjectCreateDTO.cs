using System.ComponentModel.DataAnnotations;

namespace Shared.Project
{
    public class ProjectCreateDTO
    {
        [Required]
        public int CreatedByUserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string? Technologies { get; set; }

        public ProjectCreateDTO(int createdByUserID, string title, string? technologies,string Description)
        {
            CreatedByUserID = createdByUserID;
            Title = title;
            Technologies = technologies;
            this.Description = Description;
        }
    }
}
