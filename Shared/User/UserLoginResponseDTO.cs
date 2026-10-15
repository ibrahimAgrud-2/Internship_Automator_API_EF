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
        public string   firstName{ get; set; }

        [Required]
        public string lastName { get; set; }

        [Required]
        public int PermissionType { get; set; }

        [Required]
        public bool IsActive { get; set; }


        public UserLoginResponseDTO(int userID,string firstName,string lastName, int permissionType,bool isActive)
        {
            this.UserID = userID;
            this.firstName = firstName;
            this.lastName = lastName;
            this.PermissionType = permissionType;
            this.IsActive = isActive;
        
        }
    }
}
