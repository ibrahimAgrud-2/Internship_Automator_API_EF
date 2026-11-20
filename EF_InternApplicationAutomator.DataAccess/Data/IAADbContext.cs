using EF_InternApplicationAutomator.DataAccess.Announcement;
using EF_InternApplicationAutomator.DataAccess.Application;
using EF_InternApplicationAutomator.DataAccess.ConversationIntern;
using EF_InternApplicationAutomator.DataAccess.Intern;
using EF_InternApplicationAutomator.DataAccess.Message;
using EF_InternApplicationAutomator.DataAccess.OngoingProjects;
using EF_InternApplicationAutomator.DataAccess.Project;
using EF_InternApplicationAutomator.DataAccess.Tasks;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.EntityFrameworkCore;
using System;

namespace EF_InternApplicationAutomator.DataAccess.Data
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
        public DbSet<ProjectsWithInternCount> ProjectsWithInternCounts { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<OngoingProjectEntity> OngoingProjects { get; set; }
        public DbSet<AnnouncementEntity> Announcements { get; set; }
        public DbSet<ConversationEntity> Conversations { get; set; }
        public DbSet<vw_MessagesWithContent> vw_MessagesWithContent { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<ConversationInternEntity> ConversationIntern { get; set; }
        public DbSet<vw_ConversationIntern> vw_ConversationIntern { get; set; }
        public DbSet<vw_ListGroupInterns> vw_ListGroupInterns { get; set; }
    }
}
