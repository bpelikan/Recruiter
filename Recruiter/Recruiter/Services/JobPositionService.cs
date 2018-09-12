using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recruiter.Models;
using Recruiter.Repositories;

namespace Recruiter.Services
{
    public class JobPositionService : IJobPositionService
    {
        private readonly IJobPositionRepository _jobPositionRepository;

        public JobPositionService(IJobPositionRepository jobPositionRepository)
        {
            _jobPositionRepository = jobPositionRepository;
        }

        public async Task<IEnumerable<JobPosition>> GetAllAsync()
        {
            var jobPositions = await _jobPositionRepository.GetAllAsync();
            return jobPositions;
        }

        public async Task<JobPosition> GetAsync(string id)
        {
            var jobPosition = await _jobPositionRepository.GetAsync(id);
            return jobPosition;
        }
    }
}
