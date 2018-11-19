﻿using Microsoft.AspNetCore.Http;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IMyApplicationService
    {
        IEnumerable<MyApplicationsViewModel> GetMyApplications(string userId);
        Task<MyApplicationDetailsViewModel> GetMyApplicationDetails(string applicationId, string userId);
        Task DeleteMyApplication(string applicationId, string userId);
        Task<ApplyApplicationViewModel> GetApplyApplicationViewModel(string jobPositionId, string userId);
        Task<Application> ApplyMyApplication(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel, string userId);
    }
}
