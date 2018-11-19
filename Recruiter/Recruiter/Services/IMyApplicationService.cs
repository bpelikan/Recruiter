using Recruiter.Models.MyApplicationViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IMyApplicationService
    {
        IEnumerable<MyApplicationsViewModel> GetViewModelForMyApplications(string userId);
    }
}
