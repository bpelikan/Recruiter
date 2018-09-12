using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class Application
    {
        public string Id { get; set; }

        public string CvId { get; set; }
        public virtual Cv Cv { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public string JobPositionId { get; set; }
        public virtual JobPosition JobPosition { get; set; }

    }
}
