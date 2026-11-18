using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class ChangePasswordDTO
    {


        [Required]
        [MaxLength(100)]
        public string OldPassword { get; set; }


        [Required]
        [MaxLength(100)]
        public string NewPassword { get; set; }

        public ChangePasswordDTO(string oldPassword, string newPassword)
        {

            OldPassword = oldPassword;
            NewPassword = newPassword;
        }
    }
}
