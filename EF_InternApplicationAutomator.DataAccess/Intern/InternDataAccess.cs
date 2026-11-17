using EF_InternApplicationAutomator.DataAccess.Data;
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
       
        public InternCardResponseDTO Find(int internID)
        {
            InternEntity intern = _Context.Interns.Include(i => i.Person).FirstOrDefault(i => i.InternID == internID);
            if (intern == null)
            {
                return null;
            }

            return new InternCardResponseDTO(intern.InternID, intern.Person.FirstName, intern.Person.LastName, intern.Person.Email, intern.Person.Phone, intern.StartDate, intern.EndDate, intern.IsActive);
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
                intern.CreatedDate, intern.IsActive);
        }

        public int AddIntern(InternCreateDTO intern)
        {
            //IsActive 1 means the intern is active/new meanwhile 0 means separated
            InternEntity internEntity = new InternEntity(0, intern.PersonID,
                Shared.Common.CommonTools.ComputeHash(intern.PasswordHash), intern.StartDate, intern.EndDate,
                intern.CreatedDate, 1);
            EntityEntry<InternEntity> addedIntern = _Context.Interns.Add(internEntity);

            if (_Context.SaveChanges() > 0)
            {
                return internEntity.InternID;
            }

            return -1;
        }

        public bool UpdateIntern(int internID, InternUpdateDTO internDTO)
        {
            var intern = _Context.Interns.Find(internID);
            if (intern == null || internDTO == null)
            {
                return false;
            }

            intern.PersonID = internDTO.PersonID;
            intern.StartDate = internDTO.StartDate;
            intern.EndDate = internDTO.EndDate;
            intern.CreatedDate = internDTO.CreatedDate;
            

            if (_Context.SaveChanges() > 0)
            {
                return true;
            }

            return false;
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
            return _Context.Interns.Any(intern => intern.InternID == internID && intern.IsActive);
        }
        public bool SeparateIntern(int internID)
        {
            var intern = _Context.Interns.Find(internID);
            if (internID <= 0 || intern == null)
            {
                return false;
            }
            intern.IsActive = false;
            intern.EndDate = DateTime.Now.Date;

            return _Context.SaveChanges() > 0;
        }


        public int GetInternIDByPersonID(int personID)
        {
            int internId = _Context.Interns
                .Where(i => i.PersonID == personID)
                .Select(i => i.InternID)
                .FirstOrDefault();
                     return internId;

        }
    }
}