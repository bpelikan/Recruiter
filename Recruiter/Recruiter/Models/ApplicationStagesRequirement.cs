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

        [Display(Name = "Is Phone Call Required")]
        public bool IsPhoneCallRequired { get; set; }

        [Display(Name = "Is Homework Required")]
        public bool IsHomeworkRequired { get; set; }

        [Display(Name = "Is Interview Required")]
        public bool IsInterviewRequired { get; set; }

        public string JobPositionId { get; set; }
        public virtual JobPosition JobPosition { get; set; }
    }
}
