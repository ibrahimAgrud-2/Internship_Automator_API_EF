using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Intern
{
    public class InternLoginDTO
    {



        [Required]
        [MaxLength(100)]
        public string Email { get; set; }    
        public string Password { get; set; }

        public InternLoginDTO(string email,string password)
        {
            this.Email = email;
            this.Password = password;
        }
    }
}
