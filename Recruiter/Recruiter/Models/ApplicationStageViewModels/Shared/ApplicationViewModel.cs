using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels.Shared
{
    public class ApplicationViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Created at")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Cv file name")]
        public string CvFileName { get; set; }
        [Display(Name = "Cv file url")]
        public string CvFileUrl { get; set; }

        [Display(Name = "Job Position")]
        public virtual JobPositionViewModel JobPosition { get; set; }

        [Display(Name = "User")]
        public virtual UserDetailsViewModel User { get; set; }
    }
}
