using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public class ApplicationsViewHistoriesService : IApplicationsViewHistoriesService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationsViewHistoriesService(
                    ILogger<ApplicationsViewHistoriesService> logger,
                    ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task AddApplicationsViewHistory(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing AddApplicationsViewHistory with applicationId={applicationId}. (UserID: {userId})");

            await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
            {
                Id = Guid.NewGuid().ToString(),
                ViewTime = DateTime.UtcNow,
                ApplicationId = applicationId,
                UserId = userId
            });
            await _context.SaveChangesAsync();

            //throw new NotImplementedException();
        }
    }
}
