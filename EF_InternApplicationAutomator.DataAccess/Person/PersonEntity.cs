using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.DataAccess
{
    [Table("People")]
    public class PersonEntity
    {
        [Key]
        public int PersonID { get; set; }

        [Required]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        [MaxLength(250)]
        public string ImagePath { get; set; }


        [Required]
        public bool IsActive { get; set; }

        [Required]
        public string PasswordHash { get; set; }



        [MaxLength(10)]
        public string Role { get; set; }

        [Required]
        public bool MustChangePassword { get; set; }

        public PersonEntity(int personID, string firstName, string lastName, string email, string phone, string address, string imagePath,string role,bool isActive,string passwordHash,bool mustChangePassword)
        {
            PersonID = personID;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            Address = address;
            ImagePath = imagePath;
            this.Role = role;
            this.IsActive = isActive;
            this.PasswordHash = passwordHash;
            this.MustChangePassword = mustChangePassword;
        }
    }
}
