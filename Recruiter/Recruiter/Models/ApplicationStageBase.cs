using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class ApplicationStageBase
    {
        public ApplicationStageBase()
        {
            Level = 0;
            State = ApplicationStageState.Waiting;
            Accepted = false;
        }

        public string Id { get; set; }
        public int Level { get; set; }
        [Display(Name = "State")]
        public ApplicationStageState State { get; set; }
        [Display(Name = "Accepted")]
        public bool Accepted { get; set; }
        [Display(Name = "Note")]
        public string Note { get; set; }
        [Display(Name = "Rate")]
        public int? Rate { get; set; }

        public string ApplicationId { get; set; }   
        public virtual Application Application { get; set; }

        public string ResponsibleUserId { get; set; }
        [Display(Name = "Responsible User")]
        public virtual ApplicationUser ResponsibleUser { get; set; }

        public string AcceptedById { get; set; }
        [Display(Name = "Accepted By")]
        public virtual ApplicationUser AcceptedBy { get; set; }

    }

    public class ApplicationApproval : ApplicationStageBase
    {
        public ApplicationApproval()
        {
            Level = 1;
        }
    }

    public class PhoneCall : ApplicationStageBase
    {
        public PhoneCall()
        {
            Level = 2;
        }
    }

    public class Homework : ApplicationStageBase
    {
        public Homework()
        {
            Level = 3;
        }
        [Display(Name = "Homework State")]
        public HomeworkState HomeworkState { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Duration")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Url")]
        public string Url { get; set; }

        [Display(Name = "Sending Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? SendingTime { get; set; }
    }

    public class Interview : ApplicationStageBase
    {
        public Interview()
        {
            Level = 4;
        }

        public InterviewState InterviewState { get; set; }

        public virtual ICollection<InterviewAppointment> InterviewAppointments { get; set; }
    }

    public enum ApplicationStageState
    {
        [Display(Name = "Waiting")]
        Waiting,
        [Display(Name = "In Progress")]
        InProgress,
        [Display(Name = "Finished")]
        Finished
    }

    public enum HomeworkState
    {
        [Display(Name = "Waiting For Specification")]
        WaitingForSpecification,
        [Display(Name = "Waiting For Read")]
        WaitingForRead,
        [Display(Name = "Waiting For Send Homework")]
        WaitingForSendHomework,
        [Display(Name = "Completed")]
        Completed
    }

    public enum InterviewState
    {
        [Display(Name = "Waiting For Setting Appointments")]
        WaitingForSettingAppointments,
        [Display(Name = "Request For New Appointments")]
        RequestForNewAppointments,
        [Display(Name = "Waiting For Confirm Appointment")]
        WaitingForConfirmAppointment,
        [Display(Name = "Appointment Confirmed")]
        AppointmentConfirmed
    }
}
