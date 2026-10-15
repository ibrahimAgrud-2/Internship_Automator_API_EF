using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_InternApplicationAutomator.DataAccess.Tasks
{
    [Table("Tasks")]
    public class TaskEntity
    {
        [Key]
        public int TaskID { get; set; }

        [Required]
        public int CreatedByUserID { get; set; }

        [Required]
        public int OngoingProjectID { get; set; }

        [Required]
        [ForeignKey(nameof(OngoingProjectID))]
        public OngoingProjectEntity OngoingProject { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(225)]
        public string Description { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        public bool Completed { get; set; }

        
        public DateTime? CompletedDate { get; set; }

        public TaskEntity(int taskID, int createdByUserID, int ongoingProjectID,string title, string description, DateTime assignedDate,   DateTime dueDate, bool completed,  DateTime? completedDate)
        {
            TaskID = taskID;
            CreatedByUserID = createdByUserID;
            OngoingProjectID = ongoingProjectID;
            Title = title;
            Description = description;
            AssignedDate = assignedDate;
            DueDate = dueDate;
            Completed = completed;
            CompletedDate = completedDate;
        }
    }
}