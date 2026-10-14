using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.OngoingProjects
{
    public class OngoingInternProjectResponseDTO
    {
        [Required]
        public int OngoingProjectID { get; set; }

        [Required]
        public string ProjectTitle { get; set; }

        [Required]
        public string technologies { get; set; }

        [Required]
        public int ProjectID { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public OngoingInternProjectResponseDTO( int ongoingProjectID, string projectTitle,string technologies,    int projectID,      DateTime startDate,
            DateTime dueDate)
        {
            this.OngoingProjectID = ongoingProjectID;
            this.ProjectTitle = projectTitle;
            this.technologies = technologies;
            this.StartDate = startDate;
            this.DueDate = dueDate;
            this.ProjectID = projectID;
        }
    }
}
