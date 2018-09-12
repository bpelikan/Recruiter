using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recruiter.Models;

namespace Recruiter.Repositories
{
    public class InMemoryJobPositionRepository : IJobPositionRepository
    {
        private static ISet<JobPosition> _jobPositions = new HashSet<JobPosition>
        {
            new JobPosition(){
                Id = Guid.NewGuid().ToString(),
                Name = "Name1",
                Description = "Description1"
            },
            new JobPosition(){
                Id = Guid.NewGuid().ToString(),
                Name = "Name2",
                Description = "Description2"
            },
            new JobPosition(){
                Id = Guid.NewGuid().ToString(),
                Name = "Name3",
                Description = "Description3"
            },
            new JobPosition(){
                Id = Guid.NewGuid().ToString(),
                Name = "Name4",
                Description = "Description4"
            }
        };

        public async Task<JobPosition> GetAsync(string id)
        {
            return await Task.FromResult(_jobPositions.SingleOrDefault(x => x.Id == id));
        }

        public async Task<IEnumerable<JobPosition>> GetAllAsync()
        {
            return await Task.FromResult(_jobPositions);
        }

        public async Task AddAsync(JobPosition jobPosition)
        {
            _jobPositions.Add(jobPosition);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(JobPosition jobPosition)
        {
            await Task.CompletedTask;
        }

        public async Task RemoveAsync(string id)
        {
            var jobPosition = await GetAsync(id);
            _jobPositions.Remove(jobPosition);
            await Task.CompletedTask;
        }
    }
}
