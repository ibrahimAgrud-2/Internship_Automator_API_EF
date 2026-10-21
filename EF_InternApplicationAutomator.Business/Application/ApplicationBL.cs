
using EF_InternApplicationAutomator.DataAccess.Application;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared;
using Shared.Application;
using Shared.Intern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_InternApplicationAutomator.Business.Application
{
    public class ApplicationBL
    {

        public enum enStatus { newApp=0,Cancelled=1,Declined=2,Accepted=3};


         ApplicationDataAccess _ApplicationDataAccess;
        PersonBL _person;
        Intern.InternBL _Intern;
        Shared.Common.EmailService _emailService;
        public ApplicationBL(ApplicationDataAccess applicationDataAccess,PersonBL person, Intern.InternBL intern,Shared.Common.EmailService emailService)
        {
            _ApplicationDataAccess = applicationDataAccess;
            _person = person;
            _Intern = intern;
            _emailService = emailService;
        }

        public List<ApplicationListDTO> GetAllApplications()
        {
            return _ApplicationDataAccess.GetAllApplication();
        }

        public ApplicationResponseDTO Find(int appID)
        {
            return _ApplicationDataAccess.Find(appID);
        }

        public int UpdateStatus(ApplicationStatusUpdateDTO applicationStatus)
        {

            return _ApplicationDataAccess.UpdateStatus(applicationStatus.ApplicationID, applicationStatus);
        }
        public string[] ApplicationAccepted(ApplicationStatusUpdateDTO applicationStatus)
        {
            if (applicationStatus.status == (byte)enStatus.Accepted)
            {
                ApplicationResponseDTO app = Find(applicationStatus.ApplicationID);
                PersonResponseSDTO person = _person.Find(app.PersonID);
                if (person == null||UpdateStatus(applicationStatus)==-1)
                {
                    return null;
                }
                string password = Shared.Common.CommonTools.GenerateRandomPassword(8);
                if (_Intern.AddIntern(new Shared.Intern.InternCreateDTO(person.PersonID, password, DateTime.Now.Date, null, DateTime.Now))!=-1)
                {
                    if(SendEmail(person.PersonID))
                    {
                        return [password, person.Email];
                    }
                    else
                    {
                        app.Status = 0;
                         UpdateStatus(applicationStatus);
                    }
                   
                }
                else
                {
                   return  null;
                }
            }
            return null;
        }
        public bool SendEmail(int personID)
        {
            bool sent= _emailService.SendEmail("test@gmail.com", "Onaylandi", "Staj başvurusnu onaylandi."); ;
            if(!sent)
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
            //mapping

            int personID = 0;
            PersonResponseSDTO? personCreateSDTO = _person.FindByEmail(newApplicationDTO.PersonInfo.Email);
            if(personCreateSDTO==null)
            {
                personID=_person.AddPerson(newApplicationDTO.PersonInfo);

            }
            else
            {
                personID = personCreateSDTO.PersonID;
            }
          
            //mapping
            //TODO: sistemde aktif user kimse onun ID'sini alsın
            ApplicationCreateDTO application = new ApplicationCreateDTO(personID, newApplicationDTO.PersonInfo,2,null,DateTime.Now.Date, newApplicationDTO.University, newApplicationDTO.Department, newApplicationDTO.ClassYear,(byte)enStatus.newApp, newApplicationDTO.Notes, newApplicationDTO.LinkedinURL, newApplicationDTO.GithubURL);



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