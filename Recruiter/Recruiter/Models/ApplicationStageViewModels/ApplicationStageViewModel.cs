using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class ApplicationStageViewModel
    {
        public string Id { get; set; }
        public int Level { get; set; }
        public ApplicationStageState State { get; set; }
        public bool Accepted { get; set; }
        public string Note { get; set; }
        public int Rate { get; set; }

        public string ApplicationId { get; set; }

        public string ResponsibleUserId { get; set; }

        public string AcceptedById { get; set; }
    }
}
