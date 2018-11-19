using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        private readonly IApplicationStageService _applicationStageService;
        private readonly ApplicationDbContext _context;

        public MyApplicationService(
                        IMapper mapper, 
                        ILogger<MyApplicationService> logger, 
                        ICvStorageService cvStorageService,
                        IApplicationStageService applicationStageService,
                        ApplicationDbContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _cvStorageService = cvStorageService;
            _applicationStageService = applicationStageService;
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

        public async Task<ApplyApplicationViewModel> GetApplyApplicationViewModel(string jobPositionId, string userId)
        {
            _logger.LogInformation($"Executing GetApplyApplicationViewModel with jobPositionId={jobPositionId}. (UserID: {userId})");

            var offer = await _context.JobPositions.SingleOrDefaultAsync(x => x.Id == jobPositionId);
            if (offer == null)
                throw new Exception($"JobPosition with id: {jobPositionId} doesn't exist. (UserID: {userId})");

            var vm = new ApplyApplicationViewModel()
            {
                JobPositionId = offer.Id,
                JobPositionName = offer.Name,
            };

            return vm;
            //throw new NotImplementedException();
        }

        public async Task<Application> ApplyMyApplication(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel, string userId)
        {
            _logger.LogInformation($"Executing Apply. (UserID: {userId})");

            if (cv == null)
                throw new ApplicationException($"CV file not found.");

            //ModelState.AddModelError("", "CV file not found.");
            //return View(applyApplicationViewModel);

            //var userId = _userManager.GetUserId(HttpContext.User);
            using (var stream = cv.OpenReadStream())
            {
                var CvFileName = await _cvStorageService.SaveCvAsync(stream, userId, cv.FileName);
                applyApplicationViewModel.CvFileName = CvFileName;
            }

            if (Path.GetExtension(cv.FileName) != ".pdf")
            {
                throw new ApplicationException($"CV must have .pdf extension.");
                //ModelState.AddModelError("", "CV must have .pdf extension.");
                //return View(applyApplicationViewModel);
            }
            if (applyApplicationViewModel.CvFileName == null)
            {
                throw new ApplicationException($"Something went wrong during uploading CV, try again or contact with admin.");
                //ModelState.AddModelError("", "Something went wrong during uploading CV, try again or contact with admin.");
                //return View(applyApplicationViewModel);
            }

            if (await _context.Applications
                                .Where(x => x.UserId == userId && x.JobPositionId == applyApplicationViewModel.JobPositionId).CountAsync() != 0)
            {
                throw new ApplicationException($"You have already sent application to this offer.");
                //ModelState.AddModelError("", "You have already sent application to this offer.");
                //return View(applyApplicationViewModel);
            }

            var application = new Application()
            {
                Id = Guid.NewGuid().ToString(),
                CvFileName = applyApplicationViewModel.CvFileName,
                JobPositionId = applyApplicationViewModel.JobPositionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();

            await _applicationStageService.AddRequiredStagesToApplication(application.Id);

            return application;

            //throw new NotImplementedException();
        }
    }
}

//_logger.LogInformation($"Executing GetViewModelForMyApplicationDetails with applicationId={applicationId}. (UserID: {userId})");