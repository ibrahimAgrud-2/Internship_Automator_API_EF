using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.Project;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using EF_InternApplicationAutomator.DataAccess.Tasks;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.EntityFrameworkCore;
using System;
using EF_InternApplicationAutomator.DataAccess.Announcement;

namespace EF_InternApplicationAutomator.DataAccess
{
    public  class IAADbContext:DbContext
    {
        //program.CS'de connection string'i bu const ile vereceğiz
        public  IAADbContext(DbContextOptions<IAADbContext> options) : base(options)
        {

        }


        public  DbSet<PersonEntity> People { get; set; }
        public DbSet<UserEntity> Users { set; get; }
        public DbSet<ApplicationEntity> Applications { set; get; }
        public DbSet<InternEntity> Interns { set; get; }
        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<OngoingProjectEntity> OngoingProjects { get; set; }
        public DbSet<AnnouncementEntity> Announcements { get; set; }
    }
}
