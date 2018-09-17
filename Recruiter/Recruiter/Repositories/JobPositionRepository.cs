using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Recruiter.Data;
using Recruiter.Models;

namespace Recruiter.Repositories
{
    public class JobPositionRepository : IJobPositionRepository
    {
        private readonly ApplicationDbContext _context;

        public JobPositionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobPosition> GetAsync(string id)
            => await _context.JobPositions.Include(x => x.Creator).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<IEnumerable<JobPosition>> GetAllAsync()
            => await _context.JobPositions.AsQueryable().ToListAsync();

        public async Task AddAsync(JobPosition jobPosition)
        {
            await _context.JobPositions.AddAsync(jobPosition);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(JobPosition jobPosition)
        {
            _context.JobPositions.Update(jobPosition);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(JobPosition jobPosition)
        {
            _context.JobPositions.Remove(jobPosition);
            await _context.SaveChangesAsync();
        }

        //private DbSet<JobPosition> JobPositions => _context.JobPositions;
    }
}
