using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;
using Recruiter.Models.MyApplicationViewModels.Shared;

namespace Recruiter.Services
{
    public class MyApplicationService : IMyApplicationService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ICvStorageService _cvStorageService;
        private readonly ApplicationDbContext _context;

        public MyApplicationService(
                        IMapper mapper, 
                        ILogger<MyApplicationService> logger, 
                        ICvStorageService cvStorageService, 
                        ApplicationDbContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _cvStorageService = cvStorageService;
            _context = context;
        }

        public IEnumerable<MyApplicationsViewModel> GetMyApplications(string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForMyApplications. (UserID: {userId})");

            var applications = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Where(x => x.UserId == userId);

            var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<MyApplicationsViewModel>>(applications);
            foreach (var application in vm)
                application.CreatedAt = application.CreatedAt.ToLocalTime();

            return vm;
        }

        public async Task<MyApplicationDetailsViewModel> GetMyApplicationDetails(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForMyApplicationDetails with applicationId={applicationId}. (UserID: {userId})");

            var application = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Include(x => x.ApplicationStages)
                                        .FirstOrDefault(x => x.Id == applicationId);

            if (application == null)
                throw new Exception($"Application with id {applicationId} not found. (UserID: {userId})");
            if (userId != application.UserId)
                throw new Exception($"User with id {userId} aren't owner of application with id {applicationId}. (UserID: {userId})");

            await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
            {
                Id = Guid.NewGuid().ToString(),
                ViewTime = DateTime.UtcNow,
                ApplicationId = application.Id,
                UserId = userId
                //UserId = _userManager.GetUserId(HttpContext.User)
            });
            await _context.SaveChangesAsync();

            var vm = new MyApplicationDetailsViewModel()
            {
                Id = application.Id,
                User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                CvFileUrl = _cvStorageService.UriFor(application.CvFileName),
                CreatedAt = application.CreatedAt.ToLocalTime(),
                ApplicationViews = await _context.ApplicationsViewHistories
                                                .Where(x => x.ApplicationId == application.Id && x.UserId != userId)
                                                .CountAsync(),
                ApplicationViewsAll = await _context.ApplicationsViewHistories
                                                .Where(x => x.ApplicationId == application.Id)
                                                .CountAsync(),
                ApplicationStages = application.ApplicationStages
                                                .OrderBy(x => x.Level).ToList()
            };

            return vm;
        }

        public async Task DeleteMyApplication(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing DeleteMyApplication with applicationId={applicationId}. (UserID: {userId})");

            var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
                throw new Exception($"Application with id: {applicationId} doesn't exist. (UserID: {userId})");
            if (application.UserId != userId)
                throw new Exception($"User with id: {userId} aren't owner of application with id: {application.Id}. (UserID: {userId})");

            var delete = await _cvStorageService.DeleteCvAsync(application.CvFileName);
            if (!delete)
                throw new Exception($"Something went wrong while deleting cv in Blob: {application.CvFileName}. (UserID: {userId})");

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }
    }
}

//_logger.LogInformation($"Executing GetViewModelForMyApplicationDetails with applicationId={applicationId}. (UserID: {userId})");