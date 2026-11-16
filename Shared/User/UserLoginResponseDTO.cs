using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.User
{
    public class UserLoginResponseDTO
    {

        [Required]
        public int UserID { get; set; }


        [Required]
        public int PersonID { get; set; }

        [Required]
        public string   firstName{ get; set; }

        [Required]
        public string lastName { get; set; }

        [Required]
        public bool IsActive { get; set; }


        public UserLoginResponseDTO(int userID, int personID, string firstName,string lastName,bool isActive)
        {
            this.PersonID = personID;
            this.UserID = userID;
            this.firstName = firstName;
            this.lastName = lastName;
            this.IsActive = isActive;
        
        }
    }
}
