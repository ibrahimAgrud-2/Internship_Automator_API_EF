using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class PersonResponseSDTO
    {

        [Required]
        public int PersonID { get; set; }

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

        public string ImagePath { get; set; }
        public string Role { get; set; }
        public PersonResponseSDTO(int personID,string firstName, string lastName, string email,
                string phone, string address, string imagePath,string role)
        {
            this.PersonID = personID;
            this.Role = role;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            this.Phone = phone;
            Address = address;
            ImagePath = imagePath;
        }
    }
}
