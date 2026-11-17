using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Application
{
    public class ApplicationDetailedListDTO
    {
        [Required]
        public int ApplicationID { set; get; }

        [Required]
        public short Status { get; set; }

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
        public string Notes { get; set; }


        [Required]
        public string LinkedinURL { get; set; }


        [Required]
        public string GitHubURL { get; set; }


        public ApplicationDetailedListDTO(int applicationID, short status,DateTime applicationDate, string university, string department,short classYear,string notes,string linkedinURL,string gitHubURL)
        {

            this.University = university;
            this.Department = department;
            this.ClassYear = classYear;
            this.ApplicationID = applicationID;
            this.ApplicationDate = applicationDate;
            this.Status = status;
            this.LinkedinURL = linkedinURL;
            this.GitHubURL = gitHubURL;
            this.Notes = notes;


        }
    }
}
