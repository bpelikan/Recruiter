using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class InterviewAppointment
    {
        public InterviewAppointment()
        {
            ScheduledNotification = false;
        }

        public string Id { get; set; }

        [Display(Name = "Interview Appointment State")]
        public InterviewAppointmentState InterviewAppointmentState { get; set; }

        public string InterviewId { get; set; }
        public virtual Interview Interview { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "Duration")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Accepted By Recruit")]
        public bool AcceptedByRecruit { get; set; }

        [Display(Name = "Accepted By Recruit Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? AcceptedByRecruitTime { get; set; }

        [Display(Name = "Scheduled Notification")]
        public bool ScheduledNotification { get; set; }
    }

    public enum InterviewAppointmentState
    {
        [Display(Name = "Waiting To Add")]
        WaitingToAdd,
        [Display(Name = "Waiting For Confirm")]
        WaitingForConfirm,
        [Display(Name = "Confirmed")]
        Confirmed,
        [Display(Name = "Rejected")]
        Rejected,
        [Display(Name = "Finished")]
        Finished
    }
}
