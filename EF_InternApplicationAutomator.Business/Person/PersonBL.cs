using EF_InternApplicationAutomator.Business.Intern;
using EF_InternApplicationAutomator.Business.User;
using EF_InternApplicationAutomator.DataAccess;
using Shared;
using System;

namespace EF_InternApplicationAutomator.Business
{

    public class PersonBL
    {

        public int personID { get; set; }
        PersonDataAccess _PersonDataAccess;
        InternBL _Intern;
        UserBL _User;

        //PersonBL ile DAL'ı bağlıyor.
        public PersonBL(PersonDataAccess personDataAccess,InternBL intern,UserBL user)
        {
            _PersonDataAccess = personDataAccess;
            _Intern = intern;
            _User = user;

        }
        public PersonResponseWithIDsDTO? Find(string email)
        {


            PersonResponseSDTO? p = _PersonDataAccess.FindByEmail(email);
            if (p == null)
            {
                return null;
            }

            int internID = _Intern.GetInternIDByPersonID(p.PersonID); ;
            int userID = _User.GetUserIDByPersonID(p.PersonID);

            return new PersonResponseWithIDsDTO(p.PersonID, p.FirstName, p.LastName, p.Email, p.Phone, p.Address, p.ImagePath, p.Role, p.IsActive, p.Password, internID, userID, p.MustChangePassword);
        }
        public PersonResponseSDTO? Find(int ID)
        {


            PersonResponseSDTO? p = _PersonDataAccess.Find(ID);
            if (p == null)
            {
                return null;
            }

            int internID = _Intern.GetInternIDByPersonID(p.PersonID); ;
            int userID = _User.GetUserIDByPersonID(p.PersonID);

            return p;
        }

        public int AddPerson(PersonCreateWithRoleDTO person)
        {
            return _PersonDataAccess.AddPerson(person);
        }
        public bool IsPersonExists(int personID)
        {
            return _PersonDataAccess.IsPersonExist(personID);
        }
        public bool IsPersonActive(int personID)
        {
            return _PersonDataAccess.IsPersonActive(personID);
        }
    }
}
