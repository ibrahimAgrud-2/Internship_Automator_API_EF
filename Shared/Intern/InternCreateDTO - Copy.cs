using System.ComponentModel.DataAnnotations;

namespace Shared.Intern
{
    public class InternUpdateDTO
    {
        [Required]
        public int PersonID { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public InternUpdateDTO(int personID, string passwordHash, DateTime startDate,
            DateTime? endDate, DateTime createdDate, bool isActive)
        {
            PersonID = personID;
            StartDate = startDate;
            EndDate = endDate;
            CreatedDate = createdDate;
            IsActive = isActive;
        }
    }
}