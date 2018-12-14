using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.EmailNotificationViewModel
{
    public class InterviewAppointmentToConfirmViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime StartTime { get; set; }

        public int Duration { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime EndTime { get; set; }

        public string ConfirmationUrl { get; set; }
    }
}
