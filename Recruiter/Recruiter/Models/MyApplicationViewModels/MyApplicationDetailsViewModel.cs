using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Recruiter.Models.MyApplicationViewModels.Shared;

namespace Recruiter.Models.MyApplicationViewModels
{
    public class MyApplicationDetailsViewModel
    {
        public string Id { get; set; }
        [Display(Name = "User")]
        public virtual UserDetailsViewModel User { get; set; }

        [Display(Name = "Job Position")]
        public virtual JobPositionViewModel JobPosition { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStages { get; set; }
        public virtual InterviewAppointment ConfirmedInterviewAppointment { get; set; }

        [Display(Name = "Cv file url")]
        public string CvFileUrl { get; set; }

        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Views")]
        public int ApplicationViews { get; set; }
        public int ApplicationViewsAll { get; set; }
    }
}
