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

        public async Task SendInterviewReminderQueueMessageAsync(string email, InterviewAppointment appointment)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["queueeConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("interviewappointmentqueue");
            await queue.CreateIfNotExistsAsync();

            var message = new InterviewReminderQueueMessage()
            {
                Email = email,
                InterviewAppointmentId = appointment.Id,
                StartTime = appointment.StartTime,
                Duration = appointment.Duration,
                EndTime = appointment.EndTime
            };

            var tick = (message.StartTime - DateTime.UtcNow).Ticks;

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


        private class InterviewReminderQueueMessage
        {
            public string Email { get; set; }
            public string InterviewAppointmentId { get; set; }
            public DateTime StartTime { get; set; }
            public int Duration { get; set; }
            public DateTime EndTime { get; set; }
        }

    }
}
