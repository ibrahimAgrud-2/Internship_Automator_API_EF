using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Intern
{
    public class InternCardResponseDTO
    {
        public int InternID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte Status { get; set; }

        public InternCardResponseDTO(
            int internID,
            string firstName,
            string lastName,
            string email,
            string phone,
            DateTime startDate,
            DateTime? endDate,
            byte status)
        {
            InternID = internID;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
          
        }

    }
}
