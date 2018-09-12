using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Repositories
{
    public interface IJobPositionRepository
    {
        Task<JobPosition> GetAsync(string id);
        Task<IEnumerable<JobPosition>> GetAllAsync();
        Task AddAsync(JobPosition jobPosition);
        Task UpdateAsync(JobPosition jobPosition);
        Task RemoveAsync(string id);
    }
}
