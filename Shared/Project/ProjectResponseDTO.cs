using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Project
{
    public class ProjectResponseDTO
    {
        [Required]
        public int ProjectID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }


        [MaxLength(100)]
        public string? Technologies { get; set; }

        public ProjectResponseDTO(int projectID, string title, string description, string? technologies)
        {
            ProjectID = projectID;
            Title = title;
            Technologies = technologies;
            this.Description = description;

        }
    }
}
