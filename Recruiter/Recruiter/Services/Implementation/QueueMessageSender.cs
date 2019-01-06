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
    public class QueueMessageSender : IQueueMessageSender
    {
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer<QueueMessageSender> _stringLocalizer;

        public QueueMessageSender(IConfiguration configuration, IStringLocalizer<QueueMessageSender> stringLocalizer)
        {
            _configuration = configuration;
            _stringLocalizer = stringLocalizer;
        }

        public async Task SendAppointmentReminderAsync(InterviewAppointment appointment, int time)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["GenerateAppointmentReminderQueueConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("generateappointmentreminderqueue");
            await queue.CreateIfNotExistsAsync();

            var message = new AppointmentReminderMessage()
            {
                Email = appointment.Interview.Application.User.Email,
                InterviewAppointmentId = appointment.Id,
                JobPositionName = appointment.Interview.Application.JobPosition.Name,
                NotificationTime = appointment.StartTime.Subtract(TimeSpan.FromHours(time)),
                StartTime = appointment.StartTime,
                Duration = appointment.Duration,
                EndTime = appointment.EndTime,
            };
            message.Subject = _stringLocalizer["{0} - Interview appointment reminder", message.JobPositionName];
            message.Content = SharedEmailTemplate.GetEmailTemplate(
                                    _stringLocalizer["Interview appointment reminder <br/>{0}", message.JobPositionName], 
                                    _stringLocalizer["Interview appointment reminder: StartTime: {0}", message.StartTime]);

            //var tick = (message.Appointment.StartTime - DateTime.UtcNow).Ticks;
            var test = JsonConvert.SerializeObject(message);
            CloudQueueMessage queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(message));
            //await queue.AddMessageAsync(queueMessage);
            await queue.AddMessageAsync(
                            queueMessage,
                            timeToLive: null,
                            initialVisibilityDelay: null,
                            options: null,
                            operationContext: null);

            //throw new NotImplementedException();
        }


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
