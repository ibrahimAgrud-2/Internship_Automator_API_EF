using EF_InternApplicationAutomator.DataAccess;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EF_InternApplicationAutomator.Business
{

    public class PersonBL
    {

        public int personID { get; set; }
        PersonDataAccess _PersonDataAccess;

        //PersonBL ile DAL'ı bağlıyor.
        public PersonBL(PersonDataAccess personDataAccess)
        {
            _PersonDataAccess = personDataAccess;

        }

        public List<PersonResponseSDTO> GetAllPeople()
        {
            return _PersonDataAccess.GetAllPeople();
        }

        public PersonResponseSDTO? Find(int personID)
        {
            return _PersonDataAccess.Find(personID);
        }

        public PersonResponseSDTO? FindByEmail(string email)
        {
            return _PersonDataAccess.FindByEmail(email);
        }
        public int UpdatePerson(int personID,PersonCreateSDTO personCreateSDTO)
        {
            return _PersonDataAccess.UpdatePerson(personID,personCreateSDTO);
        }

        public int AddPerson(PersonCreateSDTO newPerson)
        {
            return _PersonDataAccess.AddPerson(newPerson);
        }

        public bool DeletePerson(int personID)
        {
            return _PersonDataAccess.DeletePerson(personID);
        }



        public  bool IsPersonExistByEmail(string email)
        {
            return _PersonDataAccess.IsPersonExistByEmail(email);
        }

    }
}
