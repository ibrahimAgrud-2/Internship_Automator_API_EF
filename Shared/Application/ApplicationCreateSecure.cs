
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text;


namespace Shared.Application
{

    //normalde Add app yaparlen personID dışardan gelmemeli. işte bu dışardan creat bilgileri için önemli.
    public class ApplicationCreateSecure
    {



        public PersonCreateSDTO PersonInfo { get; set; } = new();

        [Required]
        [MaxLength(100)]
        public string University { get; set; }

        [Required]
        [MaxLength(30)]
        public string Department { get; set; }

        [Required]
        public short ClassYear { get; set; }

        [MaxLength(150)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string LinkedinURL { get; set; }

        [MaxLength(50)]
        public string GithubURL { get; set; }

        public IFormFile CV { get; set; }

        public bool HasCv { get; set; }
        public ApplicationCreateSecure()
        {
           
        }
        public ApplicationCreateSecure(PersonCreateSDTO PersonInfo, string university, string department,
            short classYear, string notes, string linkedinURL, string githubURL, bool hasCv)
        {


            this.PersonInfo = PersonInfo;
            this.University = university;
            this.Department = department;
            this.ClassYear = classYear;
            this.Notes = notes;
            this.LinkedinURL = linkedinURL;
            this.GithubURL = githubURL;
            HasCv = hasCv;
        }
    }
}
