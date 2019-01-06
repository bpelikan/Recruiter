using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.MyApplicationViewModels
{
    public class ScheduleInterviewAppointmentReminderViewModel
    {
        public virtual InterviewAppointment InterviewAppointment { get; set; }

        [Display(Name = "Time")]
        [Range(1, int.MaxValue, ErrorMessage = "Time must be greater than 0")]
        public int Time { get; set; }
    }
}
