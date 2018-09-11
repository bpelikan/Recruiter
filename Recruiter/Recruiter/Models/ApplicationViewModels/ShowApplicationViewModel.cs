using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationViewModels
{
    public class ShowApplicationViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string UserId { get; set; }

        public string CvUri { get; set; }
    }
}
