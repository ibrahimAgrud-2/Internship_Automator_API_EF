using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_InternApplicationAutomator.DataAccess.Intern
{
    [Table("Interns")]
    public class InternEntity
    {
        [Key]
        public int InternID { get; set; }

        [Required]
        public int PersonID { get; set; }

        [ForeignKey(nameof(PersonID))]
        public PersonEntity Person { get; set; }


        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public InternEntity(int internID, int personID,
            DateTime startDate, DateTime? endDate, DateTime createdDate, bool isActive)
        {
            InternID = internID;
            PersonID = personID;
            StartDate = startDate;
            EndDate = endDate;
            CreatedDate = createdDate;
            IsActive = isActive;
           
        }
    }
}