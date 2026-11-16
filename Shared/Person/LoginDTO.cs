using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class LoginDTO
    {
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }


        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        public LoginDTO(string email, string password)
        {
            this.Email = email;
            Password = password;
        }
    }
}
