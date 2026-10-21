using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Application;
using Shared.Intern;
using Shared.OngoingProjects;
using System;
using System.Security.Policy;

namespace EF_InternApplicationAutomator.DataAccess.Intern
{
    public class InternDataAccess
    {
        private readonly IAADbContext _Context;
        private readonly OngoingProjectDataAccess _OngoingProjectDataAccess;

        
        public InternDataAccess(IAADbContext context, OngoingProjectDataAccess ongoingProjectDataAccess)
        {
            _Context = context;
            _OngoingProjectDataAccess = ongoingProjectDataAccess;
        }

        public List<InternDetailedResponseDTO> GetDetailedInternRecord()
        {
            FormattableString formattableString = $@"
                            SELECT *
                            FROM vw_InternDetails";
            return _Context.Database
                .SqlQuery<InternDetailedResponseDTO>(formattableString).ToList(); 
        }
        public InterLoginResponseDTO? FindByPasswordAndEmail(string email,string password)
        {

            InternEntity internEntity= _Context.Interns.AsNoTracking().Where(i => i.Person.Email == email && i.PasswordHash == Shared.Common.CommonTools.ComputeHash(password)).Include(i => i.Person).FirstOrDefault();
            if (internEntity!=null)
            {
                if(internEntity.Person==null)
                {
                    return null;
                }
                return new InterLoginResponseDTO(internEntity.Person.FirstName,internEntity.Person.LastName,internEntity.InternID,internEntity.StartDate,internEntity.EndDate,internEntity.Status);
            }
            return null;
        }
        public InternCardResponseDTO Find(int internID)
        {
            InternEntity intern = _Context.Interns.Include(i => i.Person).FirstOrDefault(i=>i.InternID==internID);
            if (intern == null)
            {
                return null;
            }
            
            return new InternCardResponseDTO(intern.InternID,intern.Person.FirstName,intern.Person.LastName,intern.Person.Email,intern.Person.Phone,intern.StartDate,intern.EndDate,intern.Status);
        }

        public InternResponseDTO FindByPersonID(int personID)
        {
            InternEntity intern = (InternEntity)_Context.Interns.Where(i => i.PersonID == personID);
            if (intern == null)
            {
                return null;
            }

            return new InternResponseDTO(intern.InternID, intern.PersonID,
                intern.StartDate, intern.EndDate,
                intern.CreatedDate, intern.Status);
        }
      
        public int AddIntern(InternCreateDTO intern)
        {
            //Status 1 means the intern is active/new meanwhile 0 means separated
            InternEntity internEntity = new InternEntity(0, intern.PersonID,
                Shared.Common.CommonTools.ComputeHash(intern.password), intern.StartDate, intern.EndDate,
                intern.CreatedDate, 1);
            EntityEntry<InternEntity> addedIntern = _Context.Interns.Add(internEntity);

            if (_Context.SaveChanges() > 0)
            {
                return internEntity.InternID;
            }

            return -1;
        }

        public int UpdateIntern(int internID, InternCreateDTO internDTO)
        {
            var intern = _Context.Interns.Find(internID);
            if (intern == null || internDTO == null)
            {
                return -1;
            }

            intern.PersonID = internDTO.PersonID;
            intern.PasswordHash = internDTO.password;
            intern.StartDate = internDTO.StartDate;
            intern.EndDate = internDTO.EndDate;
            intern.CreatedDate = internDTO.CreatedDate;
          

            if (_Context.SaveChanges() > 0)
            {
                return internID;
            }

            return -1;
        }

        public bool DeleteIntern(int internID)
        {
            return _Context.Interns
                .Where(intern => intern.InternID == internID)
                .ExecuteDelete() > 0;
        }

        public bool IsInternExists(int internID)
        {
            return _Context.Interns.Any(intern => intern.InternID == internID);
        }
        public bool IsInternActive(int internID)
        {
            return _Context.Interns.Any(intern => intern.InternID == internID&&intern.Status==1);
        }
        public bool SeparateIntern(int internID)
        {
            var intern = _Context.Interns.Find(internID);
            if (internID<=0||intern==null)
            {
                return false;
            }
            intern.Status = 0;
            intern.EndDate = DateTime.Now.Date;

            return _Context.SaveChanges() > 0;
        }
    }
}