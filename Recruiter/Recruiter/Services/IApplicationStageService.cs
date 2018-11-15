using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IApplicationStageService
    {
        Task<bool> UpdateNextApplicationStageState(string applicationId);
        Task<bool> AddRequiredStagesToApplication(string applicationId);
        Task<ApplicationStageBase> GetApplicationStageBaseToShowInProcessStage(string stageId, string userId);
    }
}
