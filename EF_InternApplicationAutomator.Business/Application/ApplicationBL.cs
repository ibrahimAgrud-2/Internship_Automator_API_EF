
using EF_InternApplicationAutomator.DataAccess.Application;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared;
using Shared.Application;
using Shared.Common;
using Shared.Intern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EF_InternApplicationAutomator.Business.Application
{
    public class ApplicationBL
    {

        public enum enStatus { newApp = 0, Cancelled = 1, Declined = 2, Accepted = 3 };


        ApplicationDataAccess _ApplicationDataAccess;
        PersonBL _person;
        Intern.InternBL _Intern;
        Shared.Common.EmailService _emailService;
        public ApplicationBL(ApplicationDataAccess applicationDataAccess, PersonBL person, Intern.InternBL intern, Shared.Common.EmailService emailService)
        {
            _ApplicationDataAccess = applicationDataAccess;
            _person = person;
            _Intern = intern;
            _emailService = emailService;
        }

        public List<ApplicationDetailedListDTO> GetAllApplications()
        {
            return _ApplicationDataAccess.GetAllApplication();
        }

     
        public ApplicationResponseDTO Find(int appID)
        {
            return _ApplicationDataAccess.Find(appID);
        }




        public bool UpdateStatus(int applicationID, int reviewedByUserID,enStatus status,string? note)
        {

            return _ApplicationDataAccess.UpdateStatus(applicationID,(byte)status, reviewedByUserID,note);
        }
        public bool ApplicationAccepted(int  ID,int reviewedByUserID,string note)
        {
            ApplicationResponseDTO application = Find(ID);
            if (application.Status == (byte)enStatus.newApp)
            {
                PersonResponseSDTO? person = _person.Find(application.PersonID);
                if (person == null )
                {
                    return false;
                }
                if(!UpdateStatus(ID, reviewedByUserID, enStatus.Accepted,note))
                {
                    return false;
                }
                string password = Shared.Common.CommonTools.GenerateRandomPassword(11);
                if (_Intern.AddIntern(new Shared.Intern.InternCreateDTO(person.PersonID, password, DateTime.Now.Date, null, DateTime.Now,true)) != -1)
                {

                   
                    if (SendEmail(person.PersonID))
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Yeni eklenen stajyere (başvuru ID {application.ApplicationID}) email gönderilemedi");
                    }

                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    
        
        
        
        
        public bool SendEmail(int personID)
        {
            bool sent = _emailService.SendEmail("test@gmail.com", "Onaylandi", "Staj başvurusnu onaylandi."); ;
            if (!sent)
            {
                InternResponseDTO intern = _Intern.FindByPersonID(personID);

                _Intern.DeleteIntern(intern.InternID);

                return false;
            }
            return true;
        }

        public int AddApplication(ApplicationCreateSecure newApplicationDTO)
        {
            if (newApplicationDTO == null || newApplicationDTO.PersonInfo == null)
            {
                return -1;
            }
    
            int personID = 0;
            PersonResponseSDTO? personCreateSDTO = _person.Find(newApplicationDTO.PersonInfo.Email);
            if (personCreateSDTO == null)
            {
                PersonCreateWithRoleDTO personCreate = new PersonCreateWithRoleDTO(newApplicationDTO.PersonInfo.FirstName, newApplicationDTO.PersonInfo.LastName, newApplicationDTO.PersonInfo.Email, newApplicationDTO.PersonInfo.Phone, newApplicationDTO.PersonInfo.Address, newApplicationDTO.PersonInfo.ImagePath,"intern",CommonTools.ComputeHash(CommonTools.GenerateRandomPassword(11)));
                personID = _person.AddPerson(personCreate);

            }
            else
            {
                personID = personCreateSDTO.PersonID;
            }

            //mapping
            //applicaiton ilk Add yapıldığında reviewed değildir. IsActive new'dır yani review edilmeyi bekli
            ApplicationCreateDTO application = new ApplicationCreateDTO(personID, newApplicationDTO.PersonInfo,null, null, DateTime.Now.Date, newApplicationDTO.University, newApplicationDTO.Department, newApplicationDTO.ClassYear, (byte)enStatus.newApp,newApplicationDTO.Notes,newApplicationDTO.LinkedinURL,newApplicationDTO.GithubURL);



            return _ApplicationDataAccess.AddApplication(application);
        }

        public bool DeleteApplication(int appID)
        {
            return _ApplicationDataAccess.DeleteApplication(appID);
        }

        public bool IsApplicationExists(int appID)
        {
            return _ApplicationDataAccess.IsApplicationExists(appID);
        }
    }
}