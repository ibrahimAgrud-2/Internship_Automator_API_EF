using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.User
{
    public class UserCreateDTO
    {

        [Required]
        public PersonCreateSDTO PersonInfo { get; set; }

        [Required]
        public string UserName { get; set; }
       
 
        [Required]
        public bool IsActive { get; set; }

        public UserCreateDTO(PersonCreateSDTO personInfo, string userName, bool isActive)
        {

        
            IsActive = isActive;
            UserName = userName;
            this.PersonInfo = personInfo;
        }


    }
}
