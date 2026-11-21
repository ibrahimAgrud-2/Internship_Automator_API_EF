using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.User
{
    public class UserListResponseDTO
    {

        [Required]
        public int UserID { get; set; }
        [Required]
        public int PersonID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string ImagePath { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public UserListResponseDTO(int userID, int personID, string firstName,string lastName, string email,string address,string phone,string imagePath,string userName,DateTime createdDate, bool isActive)
        {

            PersonID = personID;
            CreatedDate = createdDate;
            IsActive = isActive;
            UserName = userName;
            this.UserID = userID;
            this.ImagePath = imagePath;
            this.Phone = phone;
            this.Address = address;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
        }

    }
}
