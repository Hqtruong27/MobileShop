using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;

namespace Web.Common
{
    public class Email
    {
        public static string ResetCode() => Guid.NewGuid().ToString();
        public static string SubjectEmail { get; set; }
        public static string BodyEmail { get; set; }

        public static void VerifySendEmail(string email, string resetEmailFor, string link)
        {
            var fromEmail = new MailAddress("c1709h@gmail.com", "resetpassword.no-reply");
            var toEmail = new MailAddress(email);
            if (resetEmailFor == "ResetPasswordAdmin")
            {
                SubjectEmail = ConfigurationManager.AppSettings.Get("SubjectEmailAdmin");
                BodyEmail = HttpUtility.UrlDecode(ConfigurationManager.AppSettings.Get("EmailbodyAdmin")) + " <a href='" + link + "'>" + "Click here" + "</a>";
            }
            else if (resetEmailFor == "ResetPasswordClient")
            {
                SubjectEmail = ConfigurationManager.AppSettings.Get("SubjectEmailClient");
                BodyEmail = HttpUtility.UrlDecode(ConfigurationManager.AppSettings.Get("EmailbodyClient")) + " <a href='" + link + "'>" + "Click here" + "</a>";
            }
            var smpt = new SmtpClient();
            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = SubjectEmail,
                Body = BodyEmail,
                IsBodyHtml = true
            };
            smpt.Send(message);
        }
    }
}
