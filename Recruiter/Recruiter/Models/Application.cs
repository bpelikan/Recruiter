using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class Application
    {
        public string Id { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Cv File Name")]
        public string CvFileName { get; set; }

        public string UserId { get; set; }
        [Display(Name = "User")]
        public virtual ApplicationUser User { get; set; }

        public string JobPositionId { get; set; }
        [Display(Name = "Job Position")]
        public virtual JobPosition JobPosition { get; set; }

        public virtual ICollection<ApplicationsViewHistory> ApplicationsViewHistories { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStages { get; set; }
    }
}
