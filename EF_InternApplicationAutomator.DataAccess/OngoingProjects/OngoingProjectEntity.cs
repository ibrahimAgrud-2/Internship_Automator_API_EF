using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.Project;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_InternApplicationAutomator.DataAccess.OngoingProjects
{
    [Table("OngoingProjects")]
    public class OngoingProjectEntity
    {
        [Key]
        public int OngoingProjectID { get; set; }

        [Required]
        public int InternID { get; set; }

        [ForeignKey(nameof(InternID))]
        public InternEntity Intern { get; set; }

        [Required]
        public int ProjectID { get; set; }


        [ForeignKey(nameof(ProjectID))]
        public ProjectEntity Project { get; set; }


        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public OngoingProjectEntity(
            int ongoingProjectID,
            int internID,
            int projectID,
            DateTime startDate,
            DateTime dueDate)
        {
            OngoingProjectID = ongoingProjectID;
            InternID = internID;
            ProjectID = projectID;
            StartDate = startDate;
            DueDate = dueDate;
        }
    }
}
