using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
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
        Task<ApplicationStageBase> GetApplicationStageBase(string stageId, string userId);
        Task<ApplicationStageBase> GetApplicationStageBaseWithInclude(string stageId, string userId);
        Task<ApplicationStageBase> GetApplicationStageBaseToShowInProcessStage(string stageId, string userId);
        Task<ProcessApplicationApprovalViewModel> GetViewModelForProcessApplicationApproval(string stageId, string userId);
        Task<ProcessPhoneCallViewModel> GetViewModelForProcessPhoneCall(string stageId, string userId);
        Task<AddHomeworkSpecificationViewModel> GetViewModelForAddHomeworkSpecification(string stageId, string userId);
        Task<ProcessHomeworkStageViewModel> GetViewModelForProcessHomeworkStage(string stageId, string userId);
        Task<ProcessInterviewViewModel> GetViewModelForProcessInterview(string stageId, string userId);

    }
}
