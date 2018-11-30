﻿using Recruiter.Models.ApplicationStageViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class ProcessPhoneCallViewModel
    {
        public ApplicationViewModel Application { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStagesFinished { get; set; }
        public PhoneCallViewModel StageToProcess { get; set; }
        public virtual ICollection<ApplicationStageBase> ApplicationStagesWaiting { get; set; }
    }

    public class PhoneCallViewModel
    {
        public string Id { get; set; }
        public string Note { get; set; }

        [Range(1, 5, ErrorMessage = "Rate must be beetween 1 and 5")]
        public int Rate { get; set; }
    }
}
