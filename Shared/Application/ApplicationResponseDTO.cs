using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; 


namespace Shared.Application
{
    public class ApplicationResponseDTO
    {
        [Required]
        public int ApplicationID { get; set; }

        [Required]
        public int PersonID { get; set; }

        /// <summary>
        /// An Nullable Value. So check it before use it
        /// </summary>
        public int? ReviewedByUserID { get; set; }

        /// <summary>
        /// An Nullable Value. So check it before use it
        /// </summary>
        public DateTime? ReviewDate { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string University { get; set; }

        [Required]
        [MaxLength(30)]
        public string Department { get; set; }

        [Required]
        public short ClassYear { get; set; }

        [Required]
        public byte Status { get; set; } 


        public ApplicationResponseDTO(int applicationID, int personID, int? reviewedByUserID,
            DateTime? reviewDate, DateTime applicationDate, string university, string department,
            short classYear, byte status)
        {
            this.ApplicationID = applicationID;
            this.PersonID = personID;
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