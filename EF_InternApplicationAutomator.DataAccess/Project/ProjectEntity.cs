using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_InternApplicationAutomator.DataAccess.Project
{
    [Table("Projects")]
    public class ProjectEntity
    {
        [Key]
        public int ProjectID { get; set; }

        [Required]
        public int CreatedByUserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string? Technologies { get; set; }

        public ProjectEntity(int projectID, int createdByUserID, string title,string description ,string? technologies)
        {
            ProjectID = projectID;
            CreatedByUserID = createdByUserID;
            Title = title;
            this.Description = description;
            Technologies = technologies;
        }
    }
}
