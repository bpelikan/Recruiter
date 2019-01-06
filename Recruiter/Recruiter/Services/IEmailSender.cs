using Recruiter.Models;
using Recruiter.Models.EmailNotificationViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailConfirmationAsync(string email, string link);
        Task SendEmailNotificationProcessApplicationApprovalAsync(string email, string link, ApplicationStageBase stage);
        Task SendEmailNotificationProcessPhoneCallAsync(string email, string link, ApplicationStageBase stage);
        Task SendEmailNotificationAddHomeworkSpecificationAsync(string email, string link, ApplicationStageBase stage);
        Task SendEmailNotificationProcessHomeworkStageAsync(string email, string link, ApplicationStageBase stage);
        Task SendEmailNotificationSendInterviewAppointmentsToConfirmAsync(string email,string link,Interview stage,IEnumerable<InterviewAppointmentToConfirmViewModel> interviewAppointmentsToConfirm);
        Task SendEmailNotificationProcessInterviewStageAsync(string email, string link, ApplicationStageBase stage);
        Task SendTestEmailNotificationAsync(string email, string link);
    }
}
