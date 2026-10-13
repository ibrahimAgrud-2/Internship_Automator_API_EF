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
        [MaxLength(100)]
        public string PasswordHash { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public byte Status { get; set; }

        public InternEntity(int internID, int personID, string passwordHash,
            DateTime startDate, DateTime? endDate, DateTime createdDate, byte status)
        {
            InternID = internID;
            PersonID = personID;
            PasswordHash = passwordHash;
            StartDate = startDate;
            EndDate = endDate;
            CreatedDate = createdDate;
            Status = status;
           
        }
    }
}