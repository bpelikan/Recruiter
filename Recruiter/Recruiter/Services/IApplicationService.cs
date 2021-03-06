﻿using Recruiter.Models;
using Recruiter.Models.ApplicationViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IApplicationService
    {
        ApplicationsGroupedByStagesViewModel GetViewModelForApplications(string stageName, string userId);
        Task<ApplicationDetailsViewModel> GetViewModelForApplicationDetails(string applicationId, string userId);
        Task DeleteApplication(string applicationId, string userId);
        Task<IEnumerable<ApplicationsViewHistory>> GetViewModelForApplicationsViewHistory(string applicationId, string userId);
    }
}
