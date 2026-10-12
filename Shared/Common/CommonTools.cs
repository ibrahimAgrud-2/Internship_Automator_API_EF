using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Shared.Common
{
    public class CommonTools
    {

        public static string GenerateRandomPassword(short passwordLength)
        {
            if (passwordLength <= 0)
                throw new ArgumentException("Şifre uzunluğu 0'dan büyük olmalıdır.", nameof(passwordLength));

            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digitChars = "0123456789";
            const string specialChars = "!@#$%^&*()-_=+[]{};:,.<>?";

            string allChars = lowerChars + upperChars + digitChars + specialChars;

            var password = new char[passwordLength];
            var random = new Random();

          
            password[0] = lowerChars[random.Next(lowerChars.Length)];
            if (passwordLength > 1) password[1] = upperChars[random.Next(upperChars.Length)];
            if (passwordLength > 2) password[2] = digitChars[random.Next(digitChars.Length)];
            if (passwordLength > 3) password[3] = specialChars[random.Next(specialChars.Length)];

          
            for (int i = 4; i < passwordLength; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

         
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

            
    }


    public class EmailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string FromName { get; set; } = "Staj Otomasyonu";
    }

    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public bool SendEmail(string sendTo, string subject, string body)
        {
            return true;
        }
    }
}
