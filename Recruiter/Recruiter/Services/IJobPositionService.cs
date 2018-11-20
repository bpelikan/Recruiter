﻿using Recruiter.Models.JobPositionViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IJobPositionService
    {
        IEnumerable<JobPositionViewModel> GetViewModelForIndexByJobPositionActivity(string jobPositionActivity, string userId);
    }
}
