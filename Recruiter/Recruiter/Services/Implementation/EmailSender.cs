using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Recruiter.Models;
using Recruiter.Models.EmailNotificationViewModel;
using Recruiter.Shared;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Recruiter.Services.Implementation
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<EmailSender> _stringLocalizer;

        public EmailSender(IConfiguration configuration, IStringLocalizer<EmailSender> stringLocalizer)
        {
            _configuration = configuration;
            _stringLocalizer = stringLocalizer;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(_configuration["SendGridKey"], subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("no-reply@recruiterbp.azurewebsites.net", "Recruiter"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Enable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.TrackingSettings = new TrackingSettings
            {
                ClickTracking = new ClickTracking { Enable = true }
            };

            return client.SendEmailAsync(msg);
        }

        public Task SendEmailConfirmationAsync(string email, string link)
        {
            string subject = _stringLocalizer["Confirm your email"];
            string title = _stringLocalizer["Confirm your email"];
            string content = _stringLocalizer["Please confirm your account by clicking this link: <a href='{0}'>link</a>", HtmlEncoder.Default.Encode(link)];

            return SendEmailAsync(email, subject, GetEmailTemplate(title, content));
            //return emailSender.SendEmailAsync(email, "Confirm your email",
            //    $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }

        public Task SendEmailNotificationProcessApplicationApprovalAsync(string email, string link, ApplicationStageBase stage)
        {
            string subject = _stringLocalizer["{0} - Application state notification", stage.Application.JobPosition.Name];
            string title = _stringLocalizer["Application state notification"];
            string content = "";
            if (stage.Accepted)
                content += _stringLocalizer["Your CV was accepted. <br/><br/>"];
            else
                content += _stringLocalizer["We regret to inform you that your CV was rejected. <br/><br/>"];
            content += _stringLocalizer["Check details by clicking this link: <a href='{0}'>link</a>", HtmlEncoder.Default.Encode(link)];

            //return Task.CompletedTask;
            return SendEmailAsync(email, subject, GetEmailTemplate(title, content));
        }

        public Task SendEmailNotificationProcessPhoneCallAsync(string email, string link, ApplicationStageBase stage)
        {
            string subject = _stringLocalizer["{0} - Application state notification", stage.Application.JobPosition.Name];
            string title = _stringLocalizer["Application state notification"];
            string content = "";
            if (stage.Accepted)
                content += _stringLocalizer["Your telephone conversation has ended positively. <br/><br/>"];
            else
                content += _stringLocalizer["We regret to inform you that your telephone conversation has ended negatively. <br/><br/>"];
            content += _stringLocalizer["Check details by clicking this link: <a href='{0}'>link</a>", HtmlEncoder.Default.Encode(link)];

            //return Task.CompletedTask;
            return SendEmailAsync(email, subject, GetEmailTemplate(title, content));
        }

        public Task SendEmailNotificationAddHomeworkSpecificationAsync(/*string email, */string link, ApplicationStageBase stage)
        {
            string subject = _stringLocalizer["{0} - Application state notification", stage.Application.JobPosition.Name];
            string title = _stringLocalizer["Application state notification"];
            string content = "";
            content += _stringLocalizer["Homework specification was added. <br/><br/>"];
            content += _stringLocalizer["Now you can read your homework details by clicking this link: <a href='{0}'>link</a>", HtmlEncoder.Default.Encode(link)];

            //return Task.CompletedTask;
            return SendEmailAsync(stage.Application.User.Email, subject, GetEmailTemplate(title, content));
        }

        public Task SendEmailNotificationProcessHomeworkStageAsync(string email, string link, ApplicationStageBase stage)
        {
            string subject = _stringLocalizer["{0} - Application state notification", stage.Application.JobPosition.Name];
            string title = _stringLocalizer["Application state notification"];
            string content = "";
            if (stage.Accepted)
                content += _stringLocalizer["Your homework was accepted. <br/><br/>"];
            else
                content += _stringLocalizer["We regret to inform you that your homework was rejected. <br/><br/>"];
            content += _stringLocalizer["Check details by clicking this link: <a href='{0}'>link</a>", HtmlEncoder.Default.Encode(link)];

            //return Task.CompletedTask;
            return SendEmailAsync(email, subject, GetEmailTemplate(title, content));
        }

        public Task SendEmailNotificationSendInterviewAppointmentsToConfirmAsync(
                                            string email,
                                            string link,
                                            Interview stage,
                                            IEnumerable<InterviewAppointmentToConfirmViewModel> interviewAppointmentsToConfirm)
        {
            string subject = _stringLocalizer["{0} - Application state notification", stage.Application.JobPosition.Name];
            string title = _stringLocalizer["Application state notification"];
            string content = "";
            content += _stringLocalizer["Interview appointments was added. <br/><br/>"];
            content += _stringLocalizer["Now you can confirm one of the appointments by clicking this link: <a href='{0}'>link</a>", HtmlEncoder.Default.Encode(link)];
            content += _stringLocalizer["<br/>Or confirm directly below:<br/>"];
            content += $"<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
            foreach (var appointment in interviewAppointmentsToConfirm.OrderBy(x => x.StartTime))
            {
                content += @"<tr><td width='300' valign='top'>";
                content +=      _stringLocalizer["{0} <b>({1} min</b>)", appointment.StartTime.ToString("dd.MM.yyyy HH: mm:ss"), appointment.Duration];
                content += "</td><td width='240' valign='top'>";
                content +=      _stringLocalizer["<a href='{0}'>Click here to confirm</a>", HtmlEncoder.Default.Encode(appointment.ConfirmationUrl)];
                content += @"</td></tr>";

            }
            content += $"</table>";

            //return Task.CompletedTask;
            return SendEmailAsync(email, subject, GetEmailTemplate(title, content));
        }

        public Task SendEmailNotificationProcessInterviewStageAsync(string email, string link, ApplicationStageBase stage)
        {
            string subject = _stringLocalizer["{0} - Recruitment process was finished", stage.Application.JobPosition.Name];
            string title = _stringLocalizer["Recruitment process was finished"];
            string content = "";
            if (stage.Accepted)
                content += _stringLocalizer["Congratulations! Your interview has ended positively. <br/>We invite you to the office of the company in order to sign the contract.<br/><br/>"];
            else
                content += _stringLocalizer["We regret to inform you that your interview has ended negatively. <br/><br/>"];

            //return Task.CompletedTask;
            return SendEmailAsync(email, subject, GetEmailTemplate(title, content));
        }

        public Task SendTestEmailNotificationAsync(string email, string link)
        {
            //return Task.CompletedTask;
            return SendEmailAsync(email, $"Test Email Notification {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}",
                GetEmailTemplate("Tutul", "Tresc"));
        }

        private static string GetEmailTemplate(string title, string content)
        {
            return SharedEmailTemplate.GetEmailTemplate(title, content);
        }
        //private static string EmailTemplate(string title, string content)
        //{
        //    return $@"
        //            <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
        //            <html xmlns='http://www.w3.org/1999/xhtml'>
        //            <head>
        //             <meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
        //             <title>Recruiter</title>
        //             <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
        //            </head>                
        //            <body style='margin: 0; padding: 0;'>
        //            <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border-collapse: collapse; border: 1px solid #cccccc;'>
        //             <tr>
        //              <td bgcolor='#ffffff' style='padding: 40px 30px 40px 30px;'>

        //                        <table border='0' cellpadding='0' cellspacing='0' width='100%'>
        //                         <tr>
        //                          <td style='color: #153643; font-family: Arial, sans-serif; font-size: 24px;'>
        //                           <b>
        //                                        {title}
        //                                    </b>
        //                          </td>
        //                         </tr>
        //                         <tr>
        //                          <td style='padding: 20px 0 30px 0; color: #153643; font-family: Arial, sans-serif; font-size: 16px; line-height: 20px;'>
        //                                    {content}
        //                          </td>
        //                         </tr>
        //                        </table>
        //              </td>

        //             </tr>
        //             <tr>
        //              <td bgcolor='#0254E6' style='padding: 30px 30px 30px 30px;'>
        //               <table border='0' cellpadding='0' cellspacing='0' width='100%'>
        //                <tr>
        //                 <td width='75%' style='color: #ffffff; font-family: Arial, sans-serif; font-size: 14px;'>
        //                  &reg; <a href='https://recruiterbp.azurewebsites.net/' style='color: #ffffff;'>Recruiter {DateTime.UtcNow.Year}</a>  

        //                  <br/>
        //                 </td>
        //                 <td align='right'>
        //                  <table border='0' cellpadding='0' cellspacing='0'>
        //                   <tr>
        //                    <td>
        //	                    <a href='https://recruiterbp.azurewebsites.net/'>
        //		                    <img src='https://recruiterbpstorage.blob.core.windows.net/static/website.png' alt='Recruiter website' width='38' height='38' style='display: block;' border='0' />
        //	                    </a>
        //                    </td>
        //                   </tr>
        //                  </table>
        //                 </td>
        //                </tr>
        //               </table>
        //              </td>
        //             </tr>
        //            </table>
        //            </body>
        //            </html>
        //            ";
        //}
    }
}
