using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PersonCreateWithRoleDTO:PersonCreateSDTO

    {
        [Required]
        public string Role { get; set; }
        public PersonCreateWithRoleDTO(string firstName, string lastName, string email,
                string phone, string address, string imagePath,string rol,string passwordHash):base(firstName,lastName,email,phone,address,imagePath, passwordHash)
        {
            this.Role = rol;
         
        }
    }
}
