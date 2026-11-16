
using System.ComponentModel.DataAnnotations;


namespace Shared.Application
{
    public class ApplicationCreateSecure
    {



        public PersonCreateSDTO PersonInfo { get; set; }

        [Required]
        [MaxLength(30)]
        public string University { get; set; }

        [Required]
        [MaxLength(30)]
        public string Department { get; set; }

        [Required]
        public short ClassYear { get; set; }



        public ApplicationCreateSecure(PersonCreateSDTO PersonInfo, string university, string department,
            short classYear)
        {
            

            this.PersonInfo = PersonInfo;
            this.University = university;
            this.Department = department;
            this.ClassYear = classYear;
;
        }
    }
}
