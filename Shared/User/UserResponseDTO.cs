using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.User
{
    public class UserResponseDTO
    {

        [Required]
        public int UserID { get; set; }
        [Required]
        public string UserName { get; set; }

        [Required]
        public int PersonID { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public UserResponseDTO(int userID, int personID, string userName, DateTime createdDate, bool isActive)
        {

            PersonID = personID;
            CreatedDate = createdDate;
            IsActive = isActive;
            UserName = userName;
            this.UserID = userID;
        }

    }
}
