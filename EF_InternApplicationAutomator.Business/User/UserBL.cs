using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Common;
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
        PersonDataAccess P;
        Shared.Common.EmailService _emailService;

        public UserBL(UserDataAccess personDataAccess, PersonDataAccess p, EmailService emailService)
        {
            _userDataAccess = personDataAccess;
            P = p;
            _emailService = emailService;
        }

        public List<UserListResponseDTO> GetAllUser()
        {
            return _userDataAccess.GetAllUser();
        }


 

        public int AddUser(UserCreateDTO newUser)
        {
            if (newUser != null)
            {
                string password = CommonTools.GenerateRandomPassword(11);
                PersonCreateWithRoleDTO personCreate = new PersonCreateWithRoleDTO(newUser.PersonInfo.FirstName, newUser.PersonInfo.LastName, newUser.PersonInfo.Email, newUser.PersonInfo.Phone, newUser.PersonInfo.Address, newUser.PersonInfo.ImagePath, "User",CommonTools.HashPassword(password));

                 int insertedPersonID =  P.AddPerson(personCreate);
                if(insertedPersonID<1)
                {
                    return -1;
                }
                int insertedUserID = _userDataAccess.AddUser(newUser, insertedPersonID);
                if (insertedUserID < 1)
                {
                    return -1;
                }
                //if (_emailService.SendEmail(personCreate.FirstName, personCreate.LastName, personCreate.ApplicantEmail, password))
                //{
                //    return insertedPersonID;
                //}
                //else
                //{
                //    Console.WriteLine($"Yeni kullanıcıya (Kullanici adi:  {newUser.UserName}) email gönderilemedi");
                //}
                return insertedUserID;
            }

            return -1;
        }

        public bool DeleteUser(int userID)
        {
            return _userDataAccess.DeleteUser(userID);
        }

        public int UpdateUser(int userID, int personID,UserCreateDTO userCreateDTO)
        {
           
            if(!P.IsPersonExist(personID))
            {
                return -1;
            }

            if (P.UpdatePerson(personID, userCreateDTO.PersonInfo)<1)
            {
                return -1;
            }

            return _userDataAccess.UpdateUser(userCreateDTO,userID);
        }



        public bool IsUserExists(int userID)
        {
            return _userDataAccess.IsUserExists(userID);
        }

        public int GetUserIDByPersonID(int personID)
        {
            return _userDataAccess.GetUserIDByPersonID(personID);
        }

        public bool IsUserActive(int userID)
        {
            return _userDataAccess.IsUserActive(userID);
        }
 
        public bool IsUserExists(string userName)
        {
            return _userDataAccess.IsUserExists(userName);
        }


        public UserResponseDTO Find(int userID)
        {
            return _userDataAccess.Find(userID);
        }
    }



}
