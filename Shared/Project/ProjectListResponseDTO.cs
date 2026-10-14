using System.ComponentModel.DataAnnotations;

namespace Shared.Project
{
    public class ProjectListResponseDTO
    {
        [Required]
        public int ProjectID { get; set; }

        [Required]
        public int CreatedByUserID { get; set; }

        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }
        

        [MaxLength(100)]
        public string? Technologies { get; set; }

        public ProjectListResponseDTO(int projectID, int createdByUserID, string userName, string title,string description, string? technologies)
        {
            ProjectID = projectID;
            CreatedByUserID = createdByUserID;
            Title = title;
            this.UserName = userName;
            Technologies = technologies;
            this.Description = description;

        }
    }
}
