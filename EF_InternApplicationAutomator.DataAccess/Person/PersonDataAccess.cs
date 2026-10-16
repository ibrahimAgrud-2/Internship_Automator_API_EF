using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared;
using System;


namespace EF_InternApplicationAutomator.DataAccess
{
    public class PersonDataAccess
    {


        ///Framework'un gerektirdiği kısımlar
        private readonly IAADbContext _Context;
        public PersonDataAccess(IAADbContext context)
        {
            _Context = context;
        }


        public List<PersonResponseSDTO> GetAllPeople()
        {
            List<PersonEntity> personList = _Context.People.ToList();

            List<PersonResponseSDTO> personDTOs = new List<PersonResponseSDTO>();



            //mapping
            foreach (var Person in personList)
            {
                personDTOs.Add(new PersonResponseSDTO(Person.PersonID, Person.FirstName, Person.LastName, Person.Email, Person.Phone, Person.Address, Person.ImagePath));
            }
            return personDTOs;
        }


        public PersonResponseSDTO? Find(int personID)
        {
            PersonEntity? Person = _Context.People.Find(personID);
            if (Person == null)
            {
                //Log
                return null;
            }
            else
            {
                return new PersonResponseSDTO(Person.PersonID, Person.FirstName, Person.LastName, Person.Email, Person.Phone, Person.Address, Person.ImagePath);
            }
        }

        public PersonResponseSDTO? FindByEmail(string email)
        {
            PersonEntity? Person = _Context.People.FirstOrDefault(p=>p.Email==email);
            if (Person == null)
            {
                //Log
                return null;
            }
            else
            {
                return new PersonResponseSDTO(Person.PersonID, Person.FirstName, Person.LastName, Person.Email, Person.Phone, Person.Address, Person.ImagePath);
            }
        }
        public int UpdatePerson(int personID, PersonCreateSDTO personCreateDTO)
        {
            var person = _Context.People.Find(personID);
            if (person == null || personCreateDTO == null)
            {
                return -1;
            }
            person.FirstName = personCreateDTO.FirstName;
            person.LastName = personCreateDTO.LastName;
            person.Email = personCreateDTO.Email;
            person.Phone = personCreateDTO.Phone;
            person.Address = personCreateDTO.Address;
            person.ImagePath = personCreateDTO.ImagePath;



            if (_Context.SaveChanges() > 0)
            {
                return personID;
            }

            return -1;
        }
       
        public int AddPerson(PersonCreateSDTO person)
        {
            PersonEntity personEntity = new PersonEntity(0,person.FirstName, person.LastName, person.Email, person.Phone, person.Address, person.ImagePath);
            /*personID identity olduğu için değeri 0 olarark verilmeli. Çünkü
            //EF, eğer ID değeri sıfırsa yani defaul değerde eşitse, o değerin 
            //DB tarafından verileceğini biliyor. Bu yüzden PersonID 0 olmlı.
            //personEntity.PersonID = 0;
            //defaul değer zaten sıfır olduğu için tekrar = 0 dememize gerek yok*/
            EntityEntry<PersonEntity> pw = _Context.People.Add(personEntity);

            if (_Context.SaveChanges() > 0)
            {
                return personEntity.PersonID;
            }

            return -1;
        }
       
        public bool DeletePerson(int personID)
        {
            return _Context.People
                    .Where(p => p.PersonID == personID)
                    .ExecuteDelete() > 0;
        }



        //------------------------------ CRUD DONE --------------------
        public bool IsPersonExistByEmail(string email)
        {
            return _Context.People.Any(p => p.Email == email);
        }
    }
}
