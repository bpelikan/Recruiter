using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Recruiter.Models;
using Recruiter.Services;

namespace Recruiter.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }

        public static Task SendEmailNotificationProcessApplicationApprovalAsync(this IEmailSender emailSender, string email, string link, ApplicationStageBase stage)
        {
            //return emailSender.SendEmailAsync(email, "ApplicationApproval - Application state notification",
            //    $"One of the stages of your application has changed the state, check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");

            string subject = $"{stage.Application.JobPosition.Name} - Application state notification";
            string title = $"ApplicationApproval - Application state notification - {stage.Accepted}";
            string content = $@"
                                One of the stages of your application has changed the state, 
                                check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                                <br/>
                                Rate: {stage.Rate}
                                <br/>
                                Note: {stage.Note}
                                <br/>
                                ";

            return emailSender.SendEmailAsync(email, subject, EmailTemplate(title,content));
                
            //$@"
                //    <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
                //    <html xmlns='http://www.w3.org/1999/xhtml'>
                //    <head>
                //        <meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
                //        <title>Demystifying Email Design</title>
                //        <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                //    </head>                
                //    <body style='margin: 0; padding: 0;'>
                //        <table border='1' cellpadding='0' cellspacing='0' width='100%'>
                //            <tr>
                //                <td>
                //                    One of the stages of your application has changed the state, 
                //                    check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                //                </td>
                //            </tr>
                //        </table>
                //    </body>");
        }

        public static Task SendEmailNotificationProcessPhoneCallAsync(this IEmailSender emailSender, string email, string link, ApplicationStageBase stage)
        {
            string subject = $"{stage.Application.JobPosition.Name} - Application state notification";
            string title = $"ApplicationApproval - Application state notification - {stage.Accepted}";
            string content = $@"
                                One of the stages of your application has changed the state, 
                                check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                                <br/>
                                Rate: {stage.Rate}
                                <br/>
                                Note: {stage.Note}
                                <br/>
                                ";

            return emailSender.SendEmailAsync(email, subject, EmailTemplate(title, content));
        }

        public static Task SendEmailNotificationAddHomeworkSpecificationAsync(this IEmailSender emailSender, string email, string link, ApplicationStageBase stage)
        {
            string subject = $"{stage.Application.JobPosition.Name} - Application state notification";
            string title = $"ApplicationApproval - Application state notification - {stage.Accepted}";
            string content = $@"
                                One of the stages of your application has changed the state, 
                                check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                                <br/>
                                Rate: {stage.Rate}
                                <br/>
                                Note: {stage.Note}
                                <br/>
                                ";

            return emailSender.SendEmailAsync(email, subject, EmailTemplate(title, content));
        }

        public static Task SendEmailNotificationProcessHomeworkStageAsync(this IEmailSender emailSender, string email, string link, ApplicationStageBase stage)
        {
            string subject = $"{stage.Application.JobPosition.Name} - Application state notification";
            string title = $"ApplicationApproval - Application state notification - {stage.Accepted}";
            string content = $@"
                                One of the stages of your application has changed the state, 
                                check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                                <br/>
                                Rate: {stage.Rate}
                                <br/>
                                Note: {stage.Note}
                                <br/>
                                ";

            return emailSender.SendEmailAsync(email, subject, EmailTemplate(title, content));
        }

        public static Task SendEmailNotificationSendInterviewAppointmentsToConfirmAsync(this IEmailSender emailSender, string email, string link, ApplicationStageBase stage)
        {
            string subject = $"{stage.Application.JobPosition.Name} - Application state notification";
            string title = $"ApplicationApproval - Application state notification - {stage.Accepted}";
            string content = $@"
                                One of the stages of your application has changed the state, 
                                check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                                <br/>
                                Rate: {stage.Rate}
                                <br/>
                                Note: {stage.Note}
                                <br/>
                                ";

            return emailSender.SendEmailAsync(email, subject, EmailTemplate(title, content));
        }

        public static Task SendEmailNotificationProcessInterviewStageAsync(this IEmailSender emailSender, string email, string link, ApplicationStageBase stage)
        {
            string subject = $"{stage.Application.JobPosition.Name} - Application state notification";
            string title = $"ApplicationApproval - Application state notification - {stage.Accepted}";
            string content = $@"
                                One of the stages of your application has changed the state, 
                                check by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>
                                <br/>
                                Rate: {stage.Rate}
                                <br/>
                                Note: {stage.Note}
                                <br/>
                                ";

            return emailSender.SendEmailAsync(email, subject, EmailTemplate(title, content));
        }

        public static Task SendTestEmailNotificationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, $"Test Email Notification {DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}",
                EmailTemplate("Tutu³", "Treœæ"));
        }

        private static string EmailTemplate(string title, string content)
        {
            return $@"
<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
	<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
	<title>Demystifying Email Design</title>
	<meta name='viewport' content='width=device-width, initial-scale=1.0'/>
</head>                
<body style='margin: 0; padding: 0;'>
<table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border-collapse: collapse; border: 1px solid #cccccc;'>
	<tr>
		<td align='center' bgcolor='#0254E6' style='padding: 0 0 0 0; '>
			<img src='https://recruiterbpstorage.blob.core.windows.net/static/career-3449422_640.png' alt='ASP.NET' width='600' height='300' style='display: block;' />
		</td>
	</tr>
	<tr>
		<td bgcolor='#ffffff' style='padding: 40px 30px 40px 30px;'>

            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
	            <tr>
		            <td style='color: #153643; font-family: Arial, sans-serif; font-size: 24px;'>
			            <b>
                            {title}
                        </b>
		            </td>
	            </tr>
	            <tr>
		            <td style='padding: 20px 0 30px 0; color: #153643; font-family: Arial, sans-serif; font-size: 16px; line-height: 20px;'>
                        {content}
		            </td>
	            </tr>
            </table>
		</td>
        
	</tr>
	<tr>
		<td bgcolor='#ee4c50' style='padding: 30px 30px 30px 30px;'>
			<table border='0' cellpadding='0' cellspacing='0' width='100%'>
				<tr>
					<td width='75%' style='color: #ffffff; font-family: Arial, sans-serif; font-size: 14px;'>
						&reg; <a href='https://recruiterbp.azurewebsites.net/' style='color: #ffffff;'>Recruiter {DateTime.UtcNow.Year}</a>  
						
						<br/>
					</td>
					<td align='right'>
						<table border='0' cellpadding='0' cellspacing='0'>
							<tr>
								<td>
									<a href='https://recruiterbp.azurewebsites.net/'>
										<img src='https://recruiterbpstorage.blob.core.windows.net/static/website.png' alt='Facebook' width='38' height='38' style='display: block;' border='0' />
									</a>
								</td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
		</td>
	</tr>
</table>
</body>
</html>
";
        }
    }
}
