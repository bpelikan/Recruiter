using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recruiter.Models;
using Recruiter.Models.EmailNotificationViewModel;

namespace Recruiter.Services.Implementation
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailConfirmationAsync(string email, string link)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailNotificationAddHomeworkSpecificationAsync(string email, string link, ApplicationStageBase stage)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailNotificationProcessApplicationApprovalAsync(string email, string link, ApplicationStageBase stage)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailNotificationProcessHomeworkStageAsync(string email, string link, ApplicationStageBase stage)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailNotificationProcessInterviewStageAsync(string email, string link, ApplicationStageBase stage)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailNotificationProcessPhoneCallAsync(string email, string link, ApplicationStageBase stage)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailNotificationSendInterviewAppointmentsToConfirmAsync(string email, string link, Interview stage, IEnumerable<InterviewAppointmentToConfirmViewModel> interviewAppointmentsToConfirm)
        {
            return Task.CompletedTask;
        }

        public Task SendTestEmailNotificationAsync(string email, string link)
        {
            return Task.CompletedTask;
        }
    }
}

