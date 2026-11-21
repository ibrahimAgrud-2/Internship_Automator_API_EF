using EF_InternApplicationAutomator.DataAccess.Data;
using EF_InternApplicationAutomator.DataAccess.Intern;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared;
using Shared.Conversation;
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


        public List<UserListResponseDTO> GetAllUser()
        {
            
            return _Context.Users
                .AsNoTracking()
                .Select(user => new UserListResponseDTO(user.UserID, user.Person.PersonID, user.UserName, $"{user.Person.FirstName} {user.Person.LastName}", user.CreatedDate, user.IsActive))
                .ToList();

        
        }


        //public UserResponseDTO Find(int userID)
        //{
        //    UserEntity user = _Context.Users.Find(userID);
        //    if (user == null)
        //    {
        //        //Log
        //        return null;
        //    }
        //    else
        //    {
        //        return new UserResponseDTO(user.UserID, user.PersonID, user.UserName,  user.CreatedDate, user.IsActive);
        //    }
        //}
        //public UserLoginResponseDTO? FindByPasswordAndUserName(string userName, string password)
        //{

        //    UserEntity userEntity = _Context.Users.AsNoTracking().Where(i => i.UserName==userName && i.PasswordHash == Shared.Common.CommonTools.ComputeHash(password)).Include(i => i.Person).FirstOrDefault();

        //    if (userEntity == null||userEntity.Person==null)
        //    {

        //        return null;
        //    }
        //   return new UserLoginResponseDTO(userEntity.UserID,userEntity.PersonID, userEntity.Person.FirstName, userEntity.Person.LastName, userEntity.IsActive);



        //}
        //public int UpdateUser(int userID, UserCreateDTO userDTO)
        //{
        //    var user = _Context.Users.Find(userID);
        //    if (user == null && userDTO == null)
        //    {
        //        return -1;
        //    }

        //    user.UserName = userDTO.UserName;
        //    user.PersonID = userDTO.PersonID;
        //    user.IsActive = userDTO.IsActive;
        //    user.CreatedDate = userDTO.CreatedDate;


        //    if (_Context.SaveChanges() > 0)
        //    {
        //        return userID;
        //    }

        //    return -1;
        //}





     

        public int AddUser(UserCreateDTO user,int personID)
        {
            UserEntity userEntity = new UserEntity(0, personID, user.UserName, DateTime.Now.Date, user.IsActive);
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

        public bool IsUserExists(int userID)
        {
           return _Context.Users.Any(u => u.UserID == userID);
        }

        public bool IsUserExists(string userName)
        {
            return _Context.Users.Any(u => u.UserName == userName);
        }
        public int GetUserIDByPersonID(int personID)
        {
            int internId = _Context.Users
                .Where(i => i.PersonID == personID)
                .Select(i => i.UserID)
                .FirstOrDefault();
            return internId;

        }

        public bool IsUserActive(int userID)
        {
            return _Context.Users.Any(u => u.UserID == userID&&u.IsActive);
        }

    }
}
