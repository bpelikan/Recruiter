using Recruiter.Models.ApplicationStageViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class AddAppointmentsToInterviewViewModel
    {
        public ApplicationViewModel Application { get; set; }

        public InterviewAppointment NewInterviewAppointment { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStagesFinished { get; set; }
        public AddAppointmentsViewModel StageToProcess { get; set; }
        public virtual ICollection<ApplicationStageBase> ApplicationStagesWaiting { get; set; }
    }

    public class AddAppointmentsViewModel
    {
        public string Id { get; set; }
        //public string Note { get; set; }
        //public int Rate { get; set; }
        public virtual ICollection<InterviewAppointment> InterviewAppointments { get; set; }
    }
}
