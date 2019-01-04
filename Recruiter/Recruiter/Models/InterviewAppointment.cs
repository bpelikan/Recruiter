using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class InterviewAppointment
    {
        public string Id { get; set; }

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

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime EndTime { get; set; }

        public bool AcceptedByRecruit { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? AcceptedByRecruitTime { get; set; }
    }

    public enum InterviewAppointmentState
    {
        WaitingToAdd,
        WaitingForConfirm,
        Confirmed,
        Rejected,
        Finished
    }
}
