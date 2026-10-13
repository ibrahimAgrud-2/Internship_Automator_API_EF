using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.User;
using Shared;
using Shared.Intern;
using Shared.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.Business.User
{
    public  class UserBL
    {
        UserDataAccess _userDataAccess;

      
        public UserBL(UserDataAccess personDataAccess)
        {
            _userDataAccess = personDataAccess;
        }

        public List<UserResponseDTO> GetAllUser()
        {
            return _userDataAccess.GetAllUser();
        }


        public UserResponseDTO Find(int userID)
        {
            return _userDataAccess.Find(userID);
        }
        public UserLoginResponseDTO? FindByPasswordAndUserName(string userName, string password)
        {
            return  _userDataAccess.FindByPasswordAndUserName(userName, password);
        }
        public int UpdateUser(int userID, UserCreateDTO userCreateDTO)
        {
            return _userDataAccess.UpdateUser(userID, userCreateDTO);
        }

        public int AddUser(UserCreateDTO newUser)
        {
            return _userDataAccess.AddUser(newUser);
        }

        public bool DeleteUser(int userID)
        {
            return _userDataAccess.DeleteUser(userID);
        }
        public bool IsUserExists(int userID)
        {
            return _userDataAccess.IsUserExists(userID);
        }
    }
}
