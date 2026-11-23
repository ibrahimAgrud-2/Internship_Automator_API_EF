using EF_InternApplicationAutomator.API.Service;
using EF_InternApplicationAutomator.DataAccess;
using EF_InternApplicationAutomator.DataAccess.Data;
using EF_InternApplicationAutomator.DataAccess.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Common;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Swagger'a "bu API Bearer token (JWT) kullanıyor" diyoruz
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Giriş yaptıktan sonra dönen token'ı yapıştır (başına 'Bearer' yazma)."
    });

    // ... ve bu şemayı isteklere uygula
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
        policy.WithOrigins("http://localhost:5173")
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
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.Conversation.ConversationBL>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.Message.MessagesDataAccess>();

builder.Services.AddScoped<EF_InternApplicationAutomator.DataAccess.ConversationIntern.ConversationInternDataAccess>();
builder.Services.AddScoped<EF_InternApplicationAutomator.Business.ConversationIntern.ConversationInternBL>();

var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()!;
builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<TokenService>();


// JWT doğrulama ayarları
string jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key ayarı bulunamadı (user-secrets).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 1) İmza: token bizim anahtarımızla mı imzalanmış?
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            // 2) Bileti bizim sistemimiz mi verdi?
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            // 3) Bilet bizim API için mi yazılmış?
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // 4) Süresi dolmuş mu?
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });


//email yapılandırma
builder.Services.AddTransient<EmailService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var settings = configuration
        .GetSection("ApplicantEmail")
        .Get<EmailSettings>() ?? new EmailSettings();

    settings.Password =
        configuration["ApplicantEmail:AppPassword"] ?? "";

    return new EmailService(settings, configuration);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("WebClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
