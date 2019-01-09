using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Recruiter.Models;
using Recruiter.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services.Implementation
{
    public class QueueMessageSenderService : IQueueMessageSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<QueueMessageSenderService> _stringLocalizer;

        public QueueMessageSenderService(IConfiguration configuration, IStringLocalizer<QueueMessageSenderService> stringLocalizer)
        {
            _configuration = configuration;
            _stringLocalizer = stringLocalizer;
        }

        public async Task SendAppointmentReminderAsync(InterviewAppointment appointment, int time)
        {
            CloudStorageAccount appointmentReminderStorageAccount = CloudStorageAccount.Parse(_configuration["GenerateAppointmentReminderQueueConnectionString"]);
            CloudQueueClient appointmentReminderQueueClient = appointmentReminderStorageAccount.CreateCloudQueueClient();
            CloudQueue appointmentReminderQueue = appointmentReminderQueueClient.GetQueueReference("generateappointmentreminderqueue");
            await appointmentReminderQueue.CreateIfNotExistsAsync();

            var appointmentReminderMessage = new AppointmentReminderMessage()
            {
                Email = appointment.Interview.Application.User.Email,
                InterviewAppointmentId = appointment.Id,
                JobPositionName = appointment.Interview.Application.JobPosition.Name,
                NotificationTime = appointment.StartTime.Subtract(TimeSpan.FromHours(time)),
                StartTime = appointment.StartTime,
                Duration = appointment.Duration,
                EndTime = appointment.EndTime,
            };
            appointmentReminderMessage.Subject = _stringLocalizer["{0} - Interview appointment reminder", appointmentReminderMessage.JobPositionName];
            appointmentReminderMessage.Content = SharedEmailTemplate.GetEmailTemplate(_stringLocalizer["Interview appointment reminder <br/>{0}", appointmentReminderMessage.JobPositionName], _stringLocalizer["Interview appointment reminder: StartTime: {0}", appointmentReminderMessage.StartTime]);
                
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(appointmentReminderMessage));
            await appointmentReminderQueue.AddMessageAsync(queueMessage);
        }

        //var test = JsonConvert.SerializeObject(appointmentReminderMessage);
        //await appointmentReminderqueue.AddMessageAsync(
        //                queueMessage,
        //                timeToLive: null,
        //                initialVisibilityDelay: null,
        //                options: null,
        //                operationContext: null);

        //throw new NotImplementedException();

        private class AppointmentReminderMessage
        {
            public string Email { get; set; }
            public DateTime NotificationTime { get; set; }
            public DateTime StartTime { get; set; }
            public int Duration { get; set; }
            public DateTime EndTime { get; set; }
            public string InterviewAppointmentId { get; set; }
            public string JobPositionName { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }

    }
}
