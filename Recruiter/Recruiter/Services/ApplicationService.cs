using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationViewModels;
using Recruiter.Models.ApplicationViewModels.Shared;

namespace Recruiter.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ICvStorageService _cvStorageService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationService(
                    ICvStorageService cvStorageService,
                    IMapper mapper,
                    ILogger<ApplicationService> logger,
                    ApplicationDbContext context)
        {
            _cvStorageService = cvStorageService;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public ApplicationsGroupedByStagesViewModel GetViewModelForApplications(string stageName, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForApplications with stageName={stageName}. (UserID: {userId})");

            var applications = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Include(x => x.ApplicationStages);

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
            //throw new NotImplementedException();
        }
    }
}
