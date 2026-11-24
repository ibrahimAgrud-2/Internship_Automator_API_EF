using EF_InternApplicationAutomator.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
            
            return _Context.Users.Include(u=>u.Person)
                .AsNoTracking()
                .Select(user => new UserListResponseDTO(user.UserID, user.Person.PersonID,user.Person.FirstName,user.Person.LastName,user.Person.Email,user.Person.Address,user.Person.Phone,user.Person.ImagePath, user.UserName, user.CreatedDate, user.IsActive))
                .ToList();

        
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
                return new UserResponseDTO(user.UserID, user.PersonID, user.UserName, user.CreatedDate, user.IsActive);
            }
        }







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
            return _Context.Users.Any(u => u.UserID == userID && u.IsActive);
        }


     
        public int UpdateUser( UserCreateDTO userDTO,int userID)
        {
            var user = _Context.Users.Find(userID);
            if (user == null && userDTO == null)
            {
                return -1;
            }
            user.UserName = userDTO.UserName;
            user.IsActive = userDTO.IsActive;
            if (_Context.SaveChanges() > 0)
            {
                return user.UserID;
            }

            return -1;
        }

    }
}
