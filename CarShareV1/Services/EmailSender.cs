using CarShareV1.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace CarShareV1.Services
{
    public class EmailSender : IEmailSender
    { 

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("alaeddinalhamoud@gmail.com", "Car Share"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }

        //Old way using system.net.mail smtpclient 
        /*
         *  private readonly ApplicationDbContext _context;
        public EmailSender(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            POJOMsgs model = new POJOMsgs();
            var Email_Settings = await _context.EmailSettings.FirstOrDefaultAsync();
            string FromEmail = Email_Settings.FromEmail;
            string _UserName = Email_Settings.UserName;
            string _PassWord = Email_Settings.Password;
            string _SMTP = Email_Settings.Smtp;
            int _Port = Email_Settings.Port;
            bool _SSL = Email_Settings.SSL;


            SmtpClient client = new SmtpClient(_SMTP)
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(_UserName, _PassWord),
                EnableSsl = _SSL
            };
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(FromEmail);
            mailMessage.To.Add(email);
            mailMessage.Body = htmlMessage;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = subject;


            client.Send(mailMessage);
            model.Flag = true;
            model.Msg = "has Been send.";

        }
        */
    }
}
