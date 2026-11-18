using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    //personID, firstName, lastName, role
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public int PersonID { get; set; }


        public string FirstName { get; set; }


        public int? InternID { get; set; }
        public int? UserID { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }

        public bool MustChangePassword { get; set; }


        public LoginResponseDTO(string token, int ID,string email, string firstName, string lastName, string role,int? internID,int? userID, bool mustChangePassword)
        {
            this.FirstName = firstName;
            LastName = lastName;
            this.PersonID = ID;
            this.Role = role;
            this.UserID = userID;
            this.InternID = internID;
            this.Token = token;
            this.MustChangePassword = mustChangePassword; ;
        }
    }
}
