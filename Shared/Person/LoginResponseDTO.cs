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

        public int PersonID { get; set; }


        public string FirstName { get; set; }


        public string LastName { get; set; }

        public string Role { get; set; }

        public LoginResponseDTO(int ID,string email, string firstName, string lastName, string role)
        {
            this.FirstName = firstName;
            LastName = lastName;
            this.PersonID = ID;
            this.Role = role;

        }
    }
}
