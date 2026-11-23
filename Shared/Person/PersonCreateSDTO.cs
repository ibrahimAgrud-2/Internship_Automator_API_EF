using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PersonCreateSDTO
    {
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


        public string? ImagePath { get; set; }


        public PersonCreateSDTO()
        {
            
        }
        public PersonCreateSDTO(string firstName, string lastName, string email,
                string phone, string address, string imagePath)
        {

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            this.Phone = phone;
            Address = address;
            ImagePath = imagePath;

        }
    }
}
