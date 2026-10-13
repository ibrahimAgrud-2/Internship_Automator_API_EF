using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Intern
{
    public class InterLoginResponseDTO
    {
        [Required]
        public int InternID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public byte Status { get; set; }

        public InterLoginResponseDTO(string name, string lastName, int internID,DateTime startDate, DateTime? endDate)
        {
            this.InternID = internID;
            this.StartDate = startDate;
            this.EndDate = EndDate;
            this.FirstName = name;
            this.LastName = lastName;
          
        }

    }
}
