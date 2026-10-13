using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.User
{
    public class UserLoginDTO
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }


        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        public UserLoginDTO(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}
