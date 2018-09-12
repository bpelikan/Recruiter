using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class JobPosition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        //public virtual ICollection<Application> Applications { get; set; }

        //public DateTime StartDate { get; set; }

        //public DateTime EndDate { get; set; }
    }
}
