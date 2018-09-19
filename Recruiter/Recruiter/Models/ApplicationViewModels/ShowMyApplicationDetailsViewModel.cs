using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationViewModels
{
    public class ShowMyApplicationDetailsViewModel
    {
        public virtual UserDetailsViewModel User { get; set; }
        public virtual JobPositionViewModel JobPosition { get; set; }
        public string CvFileUrl { get; set; }
    }
}
