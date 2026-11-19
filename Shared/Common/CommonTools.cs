using System.Text;
using System.Security.Cryptography;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;


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


        private static readonly PasswordHasher<object> _passwordHasher =
            new PasswordHasher<object>(
                Options.Create(new PasswordHasherOptions
                {
                    CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                    IterationCount = 600_000
                })
            );

        // Şifreyi hash'ler (DB'ye kaydetmek için)
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Şifre boş olamaz.");

            return _passwordHasher.HashPassword(
                new object(),
                password
            );
        }

        //bu fonksiyonda ilk password, kullanıcının verdiği password.
        //İkici password ise o kullanıcını DB'de önceden kayıt edilen şifresi 
        public static bool VerifyPassword( string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            try
            {
                var result = _passwordHasher.VerifyHashedPassword(
                    new object(),
                    storedHash,
                    password
                );

                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return false;
            }
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
        private readonly IConfiguration _configuration;

        public EmailService(
            EmailSettings settings,
            IConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;
        }

        public bool SendEmail(PersonResponseSDTO person,string tempPassword)
        {
            if (person == null || string.IsNullOrWhiteSpace(person.Email))
                return false;

            try
            {
                // 1. E-posta şablonunu oku
                string? subject = _configuration[
                    "EmailTemplates:ApplicationAccepted:Subject"];

                string? body = _configuration[
                    "EmailTemplates:ApplicationAccepted:Body"];

                if (string.IsNullOrWhiteSpace(subject) ||
                    string.IsNullOrWhiteSpace(body))
                {
                    return false;
                }

                // 2. Şablondaki alanları doldur
                body = body
                    .Replace("{FirstName}", person.FirstName ?? "")
                    .Replace("{LastName}", person.LastName ?? "")
                    .Replace("{Email}", person.Email)
                    .Replace("{Password}", tempPassword);

                // 3. E-postayı oluştur
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    _settings.FromName,
                    _settings.FromAddress));

                message.To.Add(MailboxAddress.Parse(person.Email));

                message.Subject = subject;

                message.Body = new TextPart("plain")
                {
                    Text = body
                };


                using var smtp = new SmtpClient();

                // Gmail sunucusuna bağlan
                smtp.Connect(
                    _settings.Host,
                    _settings.Port,
                    SecureSocketOptions.StartTls
                );

                // Kimlik doğrulaması
                smtp.Authenticate(
                    _settings.UserName,
                    _settings.Password
                );

                // E-postayı gönder
                smtp.Send(message);

                // Bağlantıyı kapat
                smtp.Disconnect(true);

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"E-posta gönderilemedi: {ex.Message}");
                return false;
            }
        }
    }




}
