using Microsoft.EntityFrameworkCore;
using Recruiter.Data;
using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public class ApplicationStageService : IApplicationStageService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationStageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateNextApplicationStageState(string applicationId)
        {
            var application = await _context.Applications
                                                .Include(x => x.ApplicationStages)
                                                .FirstOrDefaultAsync(x => x.Id == applicationId);
            if (application == null)
                throw new Exception($"Application with id {applicationId} not found.)");

            if (application.ApplicationStages.Count() != 0)
            {
                var nextStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State != ApplicationStageState.Finished).First();
                var prevStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State == ApplicationStageState.Finished).Last();
                if (nextStage.State == ApplicationStageState.Waiting && prevStage.Accepted)
                {
                    nextStage.State = ApplicationStageState.InProgress;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }
    }
}
