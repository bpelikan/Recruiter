using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IJobPositionService
    {
        IEnumerable<JobPositionViewModel> GetViewModelForIndexByJobPositionActivity(string jobPositionActivity, string userId);
        Task<JobPositionViewModel> GetViewModelForJobPositionDetails(string jobPositionId, string userId);
        AddJobPositionViewModel GetViewModelForAddJobPosition(string userId);
        Task<JobPosition> AddJobPosition(AddJobPositionViewModel addJobPositionViewModel, string userId);
        Task<EditJobPositionViewModel> GetViewModelForEditJobPosition(string jobPositionId, string userId);
        Task<JobPosition> UpdateJobPosition(EditJobPositionViewModel editJobPositionViewModel, string userId);
        Task RemoveJobPosition(string jobPositionId, string userId);
        Task RemoveJobPositionFromIndexView(string jobPositionId, string userId);
    }
}
