using EF_InternApplicationAutomator.DataAccess.Intern;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared;
using Shared.Intern;
using Shared.User;


namespace EF_InternApplicationAutomator.DataAccess.User
{
    public class UserDataAccess
    {

        private readonly IAADbContext _Context;
        public UserDataAccess(IAADbContext context)
        {
            _Context = context;
        }


        public List<UserResponseDTO> GetAllUser()
        {
            List<UserEntity> userList = _Context.Users.ToList();
   
            List<UserResponseDTO> userDTOs = new List<UserResponseDTO>();

            //mapping
            foreach (var user in userList)
            {
                userDTOs.Add(new UserResponseDTO(user.UserID,user.PersonID,user.UserName,user.PermissionType,user.CreatedDate,user.IsActive));
            }
            return userDTOs;
        }


        public UserResponseDTO Find(int userID)
        {
            UserEntity user = _Context.Users.Find(userID);
            if (user == null)
            {
                //Log
                return null;
            }
            else
            {
                return new UserResponseDTO(user.UserID, user.PersonID, user.UserName, user.PermissionType, user.CreatedDate, user.IsActive);
            }
        }
        public UserLoginResponseDTO? FindByPasswordAndUserName(string userName, string password)
        {

            UserEntity userEntity = _Context.Users.AsNoTracking().Where(i => i.UserName==userName && i.PasswordHash == Shared.Common.CommonTools.ComputeHash(password)).Include(i => i.Person).FirstOrDefault();
           
            if (userEntity == null||userEntity.Person==null)
            {

                return null;
            }
           return new UserLoginResponseDTO(userEntity.UserID, userEntity.Person.FirstName, userEntity.Person.LastName, userEntity.PermissionType, userEntity.IsActive);
     

            
        }
        public int UpdateUser(int userID, UserCreateDTO userDTO)
        {
            var user = _Context.Users.Find(userID);
            if (user == null && userDTO == null)
            {
                return -1;
            }

            user.UserName = userDTO.UserName;
            user.PersonID = userDTO.PersonID;
            user.PasswordHash = userDTO.PasswordHash;
            user.PermissionType = userDTO.PermissionType;
            user.IsActive = userDTO.IsActive;
            user.CreatedDate = userDTO.CreatedDate;


            if (_Context.SaveChanges() > 0)
            {
                return userID;
            }

            return -1;
        }
        public int AddUser(UserCreateDTO user)
        {
            UserEntity userEntity = new UserEntity(0,user.PersonID,user.UserName,user.PasswordHash,user.PermissionType,user.CreatedDate,user.IsActive);
            EntityEntry<UserEntity> pw = _Context.Users.Add(userEntity);

            if (_Context.SaveChanges() > 0)
            {
                return userEntity.UserID;
            }

            return -1;
        }
        public bool DeleteUser(int userID)
        {
            return _Context.Users
                    .Where(u => u.UserID == userID)
                    .ExecuteDelete() > 0;
        }

        //any, objeyi yüklemez. Sadece var/yok kontrolü yapar
        public bool IsUserExists(int userID)
        {
           return _Context.Users.Any(u => u.UserID == userID);
        }


    }
}
