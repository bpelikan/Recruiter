﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IApplicationsViewHistoriesService
    {
        Task AddApplicationsViewHistory(string applicationId, string userId);
    }
}
