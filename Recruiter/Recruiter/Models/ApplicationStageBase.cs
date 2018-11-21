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
        public ApplicationStageState State { get; set; }
        public bool Accepted { get; set; }
        public string Note { get; set; }
        public int? Rate { get; set; }

        public string ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        public string ResponsibleUserId { get; set; }
        public virtual ApplicationUser ResponsibleUser { get; set; }

        public string AcceptedById { get; set; }
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
        public HomeworkState HomeworkState { get; set; }

        public string Description { get; set; }
        public int Duration { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? StartTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndTime { get; set; }

        public string Url { get; set; }

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
        Waiting,
        InProgress,
        Finished
    }

    public enum HomeworkState
    {
        WaitingForSpecification,
        WaitingForRead,
        WaitingForSendHomework,
        Completed
    }

    public enum InterviewState
    {
        WaitingForSettingAppointments,
        RequestForNewAppointments,
        WaitingForConfirmAppointment,
        AppointmentConfirmed
    }
}
