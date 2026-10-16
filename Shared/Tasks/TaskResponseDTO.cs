using System.ComponentModel.DataAnnotations;

namespace Shared.Tasks
{
    public class TaskResponseDTO
    {
        [Required]
        public int TaskID { get; set; }

        public int CreatedByUserID { get; set; }

        public int OngoingProjectID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(225)]
        public string? Description { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime DueDate { get; set; }

        public bool Completed { get; set; }

        public DateTime? CompletedDate { get; set; }



        public TaskResponseDTO(
            int taskID,
            int createdByUserID,
            int ongoingProjectID,
            string title,
            string? description,
            DateTime assignedDate,
            DateTime dueDate,
            bool completed,
            DateTime? completedDate)
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