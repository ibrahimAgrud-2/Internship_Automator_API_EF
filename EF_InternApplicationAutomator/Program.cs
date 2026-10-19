using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.EntityFrameworkCore;
using Shared.Common;
using System;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
        policy.WithOrigins("https://localhost:7156")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

//Connection String
builder.Services.AddDbContext<IAADbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//PersonData Access'e PL nas?l eri?ebiliyor. Eri?memeli.
builder.Services.AddScoped<PersonDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.PersonBL>();


builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.User.UserDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.User.UserBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Application.ApplicationDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Application.ApplicationBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Intern.InternDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Intern.InternBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Project.ProjectDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Project.ProjectsBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Tasks.TaskDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Tasks.TasksBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.OngoingProjects.OngoingProjectDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.OngoingProjects.OngoingProjectsBL>();


builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Announcement.AnnouncementDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Announcements.AnnouncementsBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Conversation.ConversationDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.conversation.ConversationBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Messages.MessageDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Message.MessageBL>();


var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()!;
builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<EmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("WebClient");
app.UseAuthorization();

app.MapControllers();

app.Run();
