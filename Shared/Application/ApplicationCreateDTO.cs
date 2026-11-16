
using System.ComponentModel.DataAnnotations;


namespace Shared.Application
{
    public class ApplicationCreateDTO
    {

        [Required]
        public int PersonID { get; set; }
        public PersonCreateSDTO PersonInfo { get; set; }

        public int? ReviewedByUserID { get; set; }
        public DateTime? ReviewDate { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string University { get; set; }

        [Required]
        [MaxLength(30)]
        public string Department { get; set; }

        [Required]
        public short ClassYear { get; set; }

        [Required]
        public byte Status { get; set; }

       
        public ApplicationCreateDTO(int personID,PersonCreateSDTO PersonInfo, int? reviewedByUserID,
            DateTime? reviewDate, DateTime applicationDate, string university, string department,
            short classYear, byte status)
        {
            

            this.PersonID = personID;
            this.PersonInfo = PersonInfo;
            this.ReviewedByUserID = reviewedByUserID;
            this.ReviewDate = reviewDate;
            this.ApplicationDate = applicationDate;
            this.University = university;
            this.Department = department;
            this.ClassYear = classYear;
            this.Status = status;
        }
    }
}
