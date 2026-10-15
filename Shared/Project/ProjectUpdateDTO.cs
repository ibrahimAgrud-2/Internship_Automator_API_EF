using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Shared.Project
{
    public class ProjectUpdateDTO
    {

        [Required]
        [MaxLength(100)]
        public string title { get; set; }

        [Required]
        [MaxLength(100)]
        public string description { get; set; }

        [MaxLength(100)]
        public string? technologies  { get; set; }

        public ProjectUpdateDTO( string title, string? technologies, string Description)
        {
            this.title = title;
            this.technologies  = technologies;
            this.description = Description;
        }
    }
}
