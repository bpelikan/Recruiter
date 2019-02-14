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
        public int Time { get; set; }
    }
}
