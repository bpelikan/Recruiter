using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class ApplicationStagesRequirement
    {
        public string Id { get; set; }

        [Display(Name = "Is Application Approval Required")]
        public bool IsApplicationApprovalRequired { get; set; }
        public string DefaultResponsibleForApplicatioApprovalId { get; set; }
        public virtual ApplicationUser DefaultResponsibleForApplicatioApproval { get; set; }

        [Display(Name = "Is Phone Call Required")]
        public bool IsPhoneCallRequired { get; set; }
        public string DefaultResponsibleForPhoneCallId { get; set; }
        public virtual ApplicationUser DefaultResponsibleForPhoneCall { get; set; }

        [Display(Name = "Is Homework Required")]
        public bool IsHomeworkRequired { get; set; }
        public string DefaultResponsibleForHomeworkId { get; set; }
        public virtual ApplicationUser DefaultResponsibleForHomework { get; set; }

        [Display(Name = "Is Interview Required")]
        public bool IsInterviewRequired { get; set; }
        public string DefaultResponsibleForInterviewId { get; set; }
        public virtual ApplicationUser DefaultResponsibleForInterview { get; set; }

        public string JobPositionId { get; set; }
        public virtual JobPosition JobPosition { get; set; }

        public void RemoveDefaultResponsibleIfStageIsDisabled()
        {
            if (!IsApplicationApprovalRequired)
                DefaultResponsibleForApplicatioApprovalId = null;
            if (!IsPhoneCallRequired)
                DefaultResponsibleForPhoneCallId = null;
            if (!IsHomeworkRequired)
                DefaultResponsibleForHomeworkId = null;
            if (!IsInterviewRequired)
                DefaultResponsibleForInterviewId = null;
        }
    }
}
