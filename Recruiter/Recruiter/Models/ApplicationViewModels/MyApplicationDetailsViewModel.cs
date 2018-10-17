using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationViewModels
{
    public class MyApplicationDetailsViewModel
    {
        public string Id { get; set; }
        public virtual UserDetailsViewModel User { get; set; }

        public virtual JobPositionViewModel JobPosition { get; set; }

        public string CvFileUrl { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Views")]
        public int ApplicationViews { get; set; }
        public int ApplicationViewsAll { get; set; }
    }
}
