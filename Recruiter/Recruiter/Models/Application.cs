using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class Application
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CvFileName { get; set; }
        //public virtual Cv Cv { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public string JobPositionId { get; set; }
        public virtual JobPosition JobPosition { get; set; }

        public virtual ICollection<ApplicationsViewHistory> ApplicationsViewHistories { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStages { get; set; }
    }
}
