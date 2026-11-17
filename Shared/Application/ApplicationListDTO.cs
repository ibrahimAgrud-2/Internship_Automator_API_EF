using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application
{
    public class ApplicationListDTO
    {
        [Required]
        public int ApplicationID { set; get; }

        [Required]
        public short Status { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }
        public PersonCreateSDTO PersonInfo { get; set; }

        [Required]
        [MaxLength(30)]
        public string University { get; set; }

        [Required]
        [MaxLength(30)]
        public string Department { get; set; }

        [Required]
        public short ClassYear { get; set; }


        public ApplicationListDTO(PersonCreateSDTO PersonInfo,int applicationID, short status,DateTime applicationDate, string university, string department,
            short classYear)
        {


            this.PersonInfo = PersonInfo;
            this.University = university;
            this.Department = department;
            this.ClassYear = classYear;
            this.ApplicationID = applicationID;
            this.ApplicationDate = applicationDate;
            this.Status = status;


        }
    }
}
