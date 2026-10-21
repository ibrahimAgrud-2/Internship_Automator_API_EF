using System.ComponentModel.DataAnnotations;

namespace Shared.Intern
{
    public class InternCreateDTO
    {
        [Required]
        public int PersonID { get; set; }

        [Required]
        [MaxLength(100)]
        public string password { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        public InternCreateDTO(int personID, string password, DateTime startDate,
            DateTime? endDate, DateTime createdDate)
        {
            PersonID = personID;
            this.password = password;
            StartDate = startDate;
            EndDate = endDate;
            CreatedDate = createdDate;
        }
    }
}