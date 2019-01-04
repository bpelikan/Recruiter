using Recruiter.Models.ApplicationViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationViewModels
{
    public class ApplicationDetailsViewModel
    {
        public string Id { get; set; }

        [Display(Name = "User")]
        public virtual UserDetailsViewModel User { get; set; }

        [Display(Name = "Job Position")]
        public virtual JobPositionViewModel JobPosition { get; set; }

        [Display(Name = "Cv File Url")]
        public string CvFileUrl { get; set; }

        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStages { get; set; }
        public virtual ICollection<ApplicationsViewHistory> ApplicationsViewHistories { get; set; }
    }
}
