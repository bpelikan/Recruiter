﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class ProcessApplicationApprovalViewModel
    {
        public ApplicationViewModel Application { get; set; }
        public virtual ICollection<ApplicationStageBase> ApplicationStages { get; set; }

        public ApplicationApprovalViewModel StageToProcess { get; set; }
    }

    public class ApplicationApprovalViewModel
    {
        public string Id { get; set; }
        public bool Accepted { get; set; }
        public string Note { get; set; }
        public int Rate { get; set; }
    }
}
