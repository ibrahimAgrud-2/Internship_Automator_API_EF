using System.ComponentModel.DataAnnotations;

namespace Shared.Tasks
{
    public class TaskCreateDTO
    {

        [Required]
        public int CreatedByUserID { get; set; }

        [Required]
        public int OngoingProjectID { get; set; }

        [MaxLength(100)]
        [Required]
        public string Title { get; set; }

        [MaxLength(200)]
        [Required]
        public string Description { get; set; }



        [Required]
        public DateTime DueDate { get; set; }

        public TaskCreateDTO(
            int createdByUserID,
            int ongoingProjectID,
            string title,string description,

            DateTime dueDate)
        {
            CreatedByUserID = createdByUserID;
            OngoingProjectID = ongoingProjectID;
            Title = title;
          
            DueDate = dueDate;
            this.Description = description;
        }
    }
}
