using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services.Implementation
{
    public class QueueMessageSender : IQueueMessageSender
    {
        private readonly IConfiguration _configuration;

        public QueueMessageSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAppointmentReminderAsync(string email, InterviewAppointment appointment)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["GenerateAppointmentReminderQueueConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("generateappointmentreminderqueue");
            await queue.CreateIfNotExistsAsync();

            var message = new AppointmentReminderMessage()
            {
                Email = email,
                InterviewAppointmentId = appointment.Id,
                JobPositionName = appointment.Interview.Application.JobPosition.Name,
                NotificationTime = appointment.StartTime.Subtract(TimeSpan.FromMinutes(1)),
                StartTime = appointment.StartTime,
                Duration = appointment.Duration,
                EndTime = appointment.EndTime
            };

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
        }

    }
}
