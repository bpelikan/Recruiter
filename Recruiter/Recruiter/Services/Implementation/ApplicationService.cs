﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationViewModels;
using Recruiter.Models.ApplicationViewModels.Shared;

namespace Recruiter.Services.Implementation
{
    public class ApplicationService : IApplicationService
    {
        private readonly ICvStorageService _cvStorageService;
        private readonly IApplicationsViewHistoriesService _applicationsViewHistoriesService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<ApplicationService> _stringLocalizer;
        private readonly ApplicationDbContext _context;

        public ApplicationService(
                    ICvStorageService cvStorageService,
                    IApplicationsViewHistoriesService applicationsViewHistoriesService,
                    IMapper mapper,
                    ILogger<ApplicationService> logger,
                    IStringLocalizer<ApplicationService> stringLocalizer,
                    ApplicationDbContext context)
        {
            _cvStorageService = cvStorageService;
            _applicationsViewHistoriesService = applicationsViewHistoriesService;
            _mapper = mapper;
            _logger = logger;
            _stringLocalizer = stringLocalizer;
            _context = context;
        }

        public ApplicationsGroupedByStagesViewModel GetViewModelForApplications(string stageName, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForApplications with stageName={stageName}. (UserID: {userId})");

            var applications = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Include(x => x.ApplicationStages);

            var stagesSortedByName = GetApplicationCountSortedByCurrentStagesName(userId);

            var vm = new ApplicationsGroupedByStagesViewModel()
            {
                ApplicationStagesGroupedByName = stagesSortedByName,
            };
            vm.Applications = new List<ApplicationsViewModel>();
            foreach (var application in applications)
            {
                var currentStage = application.ApplicationStages.Where(x => x.State != ApplicationStageState.Finished).OrderBy(x => x.Level).FirstOrDefault();
                if (currentStage == null && application.ApplicationStages
                                                                    .Where(x => x.State == ApplicationStageState.Finished)
                                                                    .Count() == application.ApplicationStages.Count())
                {
                    if (stageName == "" || stageName == "Finished")
                    {
                        vm.Applications.Add(new ApplicationsViewModel()
                        {
                            Id = application.Id,
                            CreatedAt = application.CreatedAt.ToLocalTime(),
                            JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                            User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                            CurrentStage = "Finished",
                            CurrentStageIsAssigned = true
                        });
                    }
                }
                else
                {
                    if (stageName == "" || stageName == currentStage.GetType().Name)
                    {
                        vm.Applications.Add(new ApplicationsViewModel()
                        {
                            Id = application.Id,
                            CreatedAt = application.CreatedAt.ToLocalTime(),
                            JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                            User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                            CurrentStage = currentStage?.GetType().Name,
                            CurrentStageIsAssigned = currentStage?.ResponsibleUserId != null ? true : false
                        });
                    }
                }
            }

            return vm;
        }

        public async Task<ApplicationDetailsViewModel> GetViewModelForApplicationDetails(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForApplicationDetails with applicationId={applicationId}. (UserID: {userId})");

            var application = _context.Applications
                                .Include(x => x.JobPosition)
                                .Include(x => x.User)
                                .FirstOrDefault(x => x.Id == applicationId);
            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["Application with ID:{0} not found.", applicationId]);
            }

            await _applicationsViewHistoriesService.AddApplicationsViewHistory(applicationId, userId);
            
            var applicationStages = _context.ApplicationStages
                                        .Include(x => x.ResponsibleUser)
                                        .Include(x => x.AcceptedBy)
                                        .Where(x => x.ApplicationId == application.Id).OrderBy(x => x.Level);

            var viewHistories = await _context.ApplicationsViewHistories
                                                .Where(x => x.ApplicationId == application.Id)
                                                .OrderByDescending(x => x.ViewTime)
                                                .Take(10)
                                                .ToListAsync();
            foreach (var viewHistory in viewHistories)
                viewHistory.ViewTime = viewHistory.ViewTime.ToLocalTime();

            var vm = new ApplicationDetailsViewModel()
            {
                Id = application.Id,
                User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                CvFileUrl = _cvStorageService.UriFor(application.CvFileName),
                CreatedAt = application.CreatedAt.ToLocalTime(),
                ApplicationsViewHistories = viewHistories,
                ApplicationStages = applicationStages.ToList()
            };

            return vm;
        }

        public async Task DeleteApplication(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing DeleteApplication with applicationId={applicationId}. (UserID: {userId})");

            var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);
            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["Application with ID:{0} not found.", applicationId]);
            }

            await _cvStorageService.DeleteCvAsync(application.CvFileName);

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationsViewHistory>> GetViewModelForApplicationsViewHistory(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForApplicationsViewHistory with applicationId={applicationId}. (UserID: {userId})");

            var application = _context.Applications.FirstOrDefault(x => x.Id == applicationId);
            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["Application with ID:{0} not found.", applicationId]);
            }

            var vm = await _context.ApplicationsViewHistories
                                        .Where(x => x.ApplicationId == application.Id)
                                        .OrderByDescending(x => x.ViewTime)
                                        .ToListAsync();
            foreach (var viewHistory in vm)
                viewHistory.ViewTime = viewHistory.ViewTime.ToLocalTime();

            return vm;
        }


        private List<StagesViewModel> GetApplicationCountSortedByCurrentStagesName(string userId)
        {
            _logger.LogInformation($"Executing GetApplicationCountSortedByCurrentStagesName. (UserID: {userId})");

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                stagesSortedByName.Add(new StagesViewModel()
                {
                    Name = t.Name,
                    Quantity = _context.ApplicationStages
                                            .AsNoTracking()
                                            .Where(x => x.State == ApplicationStageState.InProgress &&
                                                        x.GetType().Name == t.Name).Count(),
                });
            }

            stagesSortedByName.Add(new StagesViewModel()
            {
                Name = "Finished",
                Quantity = _context.Applications
                            .Include(x => x.ApplicationStages)
                            .Where(x => x.ApplicationStages
                                            .Where(y => y.State == ApplicationStageState.Finished).Count() == x.ApplicationStages.Count())
                            .Count(),
            });

            return stagesSortedByName;
        }
    }
}
