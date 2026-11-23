using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess.Application
{
    [Table("Applications")]
    public class ApplicationEntity
    {
        [Key]
        public int ApplicationID { get; set; }

        [Required]
        public int PersonID { get; set; }

        public int? ReviewedByUserID { get; set; }

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



  
        public string? Notes { get; set; }


        [Required]
        public string LinkedInUrl { get; set; }


        [Required]
        public string GitHubUrl { get; set; }
        public string CVPath { get; set; }


        [Required]
        public bool HasCv { get; set; }
        public ApplicationEntity(int applicationID, int personID, int? reviewedByUserID, DateTime? reviewDate, DateTime applicationDate, string university, string department, short classYear, byte status, string notes, string linkedInUrl, string gitHubUrl, string cVPath, bool hasCv)
        {
            ApplicationID = applicationID;
            PersonID = personID;
            ReviewedByUserID = reviewedByUserID;
            ReviewDate = reviewDate;
            ApplicationDate = applicationDate;
            University = university;
            Department = department;
            ClassYear = classYear;
            Status = status;
            this.GitHubUrl = gitHubUrl;
            this.LinkedInUrl = linkedInUrl;
            this.Notes = notes;
            CVPath = cVPath;
            HasCv = hasCv;
        }
    }
}