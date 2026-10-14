using System.ComponentModel.DataAnnotations;

namespace Shared.Project
{
    public class ProjectResponseDTO
    {
        [Required]
        public int ProjectID { get; set; }

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

        public ProjectResponseDTO(int projectID, int createdByUserID, string title,string description, string? technologies)
        {
            ProjectID = projectID;
            CreatedByUserID = createdByUserID;
            Title = title;
            Technologies = technologies;
            this.Description = description;

        }
    }
}
