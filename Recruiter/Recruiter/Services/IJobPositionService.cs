using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IJobPositionService
    {
        Task<IEnumerable<JobPosition>> GetAllAsync();
        Task<JobPosition> GetAsync(string id);
    }
}
