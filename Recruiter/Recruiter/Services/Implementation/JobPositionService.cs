using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Repositories;
using Recruiter.Shared;

namespace Recruiter.Services.Implementation
{
    public class JobPositionService : IJobPositionService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly ApplicationDbContext _context;

        public JobPositionService(
                    IMapper mapper,
                    ILogger<JobPositionService> logger,
                    IJobPositionRepository jobPositionRepository,
                    ApplicationDbContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _jobPositionRepository = jobPositionRepository;
            _context = context;
        }

        public IEnumerable<JobPositionViewModel> GetViewModelForIndexByJobPositionActivity(string jobPositionActivity, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForIndexByJobPositionActivity with jobPositionActivity={jobPositionActivity}. (UserID: {userId})");

            IEnumerable<JobPosition> jobPositions = null;
            switch (jobPositionActivity)
            {
                case JobPositionActivity.Active:
                    jobPositions = _context.JobPositions.Where(x => x.StartDate <= DateTime.UtcNow &&
                                                                    (x.EndDate >= DateTime.UtcNow || x.EndDate == null));
                    break;

                case JobPositionActivity.Planned:
                    jobPositions = _context.JobPositions.Where(x => x.StartDate > DateTime.UtcNow);
                    break;

                case JobPositionActivity.Expired:
                    jobPositions = _context.JobPositions.Where(x => x.EndDate < DateTime.UtcNow);
                    break;

                default:
                    jobPositions = _context.JobPositions;
                    break;
            }

            jobPositions = jobPositions
                            .OrderByDescending(x => x.EndDate == null)
                                .ThenByDescending(x => x.EndDate)
                                    .ThenByDescending(x => x.StartDate);

            var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);

            foreach (var jobPosition in vm)
            {
                jobPosition.StartDate = jobPosition.StartDate.ToLocalTime();
                jobPosition.EndDate = jobPosition.EndDate?.ToLocalTime();
            }

            return vm;
        }

        public async Task<JobPositionViewModel> GetViewModelForJobPositionDetails(string jobPositionId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForJobPositionDetails with jobPositionId={jobPositionId}. (UserID: {userId})");


            var jobPosition = await _jobPositionRepository.GetAsync(jobPositionId);
            if (jobPosition == null)
            {
                _logger.LogError($"JobPosition with ID:{jobPositionId} doesn't exists. (UserID: {userId})");
                throw new NotFoundException($"JobPosition with ID:{jobPositionId} doesn't exists.");
            }

            var vm = _mapper.Map<JobPosition, JobPositionViewModel>(jobPosition);
            vm.StartDate = vm.StartDate.ToLocalTime();
            vm.EndDate = vm.EndDate?.ToLocalTime();

            var applications = await _context.Applications.Include(x => x.User).Where(x => x.JobPositionId == jobPosition.Id).ToListAsync();
            foreach (var application in applications)
            {
                application.CreatedAt = application.CreatedAt.ToLocalTime();
                vm.AddApplication(application);
            }

            return vm;
        }

        public AddJobPositionViewModel GetViewModelForAddJobPosition(string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForAddJobPosition. (UserID: {userId})");

            var vm = new AddJobPositionViewModel()
            {
                StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                                            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 00).ToLocalTime(),
                ApplicationStagesRequirement = new ApplicationStagesRequirement()
                {
                    IsApplicationApprovalRequired = true,
                    IsHomeworkRequired = true,
                    IsInterviewRequired = true,
                    IsPhoneCallRequired = true,
                }
            };

            return vm;
        }

        public async Task<JobPosition> AddJobPosition(AddJobPositionViewModel addJobPositionViewModel, string userId)
        {
            _logger.LogInformation($"Executing AddJobPosition. (UserID: {userId})");

            addJobPositionViewModel.ApplicationStagesRequirement.RemoveDefaultResponsibleIfStageIsDisabled();

            var jobPosition = new JobPosition()
            {
                Id = Guid.NewGuid().ToString(),
                Name = addJobPositionViewModel.Name,
                Description = addJobPositionViewModel.Description,
                StartDate = addJobPositionViewModel.StartDate.ToUniversalTime(),
                EndDate = addJobPositionViewModel.EndDate?.ToUniversalTime(),
                CreatorId = userId,
                ApplicationStagesRequirement = addJobPositionViewModel.ApplicationStagesRequirement
            };

            await _jobPositionRepository.AddAsync(jobPosition);

            var jobPositionCheck = await _jobPositionRepository.GetAsync(jobPosition.Id);
            if (jobPositionCheck == null)
                throw new Exception($"JobPositionId with ID: {jobPosition.Id} not found. (UserID: {userId})");

            return jobPositionCheck;
        }

        public async Task<EditJobPositionViewModel> GetViewModelForEditJobPosition(string jobPositionId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForEditJobPosition with jobPositionId={jobPositionId}. (UserID: {userId})");

            var jobPosition = await _jobPositionRepository.GetAsync(jobPositionId);
            if (jobPosition == null)
                throw new Exception($"Job position with id {jobPositionId} not found. (UserID: {userId})");

            var vm = _mapper.Map<JobPosition, EditJobPositionViewModel>(jobPosition);
            vm.StartDate = vm.StartDate.ToLocalTime();
            vm.EndDate = vm.EndDate?.ToLocalTime();

            return vm;
        }

        public async Task<JobPosition> UpdateJobPosition(EditJobPositionViewModel editJobPositionViewModel, string userId)
        {
            _logger.LogInformation($"Executing UpdateJobPosition. (UserID: {userId})");

            var jobPosition = await _jobPositionRepository.GetAsync(editJobPositionViewModel.Id);
            if (jobPosition == null)
                throw new Exception($"Job position with id {editJobPositionViewModel.Id} not found. (UserID: {userId})");

            jobPosition.Name = editJobPositionViewModel.Name;
            jobPosition.Description = editJobPositionViewModel.Description;
            jobPosition.StartDate = editJobPositionViewModel.StartDate.ToUniversalTime();
            jobPosition.EndDate = editJobPositionViewModel.EndDate?.ToUniversalTime();

            await _jobPositionRepository.UpdateAsync(jobPosition);

            var jobPositionCheck = await _jobPositionRepository.GetAsync(jobPosition.Id);
            if (jobPositionCheck == null)
                throw new Exception($"JobPositionId with ID: {jobPosition.Id} not found. (UserID: {userId})");

            return jobPositionCheck;
        }

        public async Task RemoveJobPosition(string jobPositionId, string userId)
        {
            _logger.LogInformation($"Executing RemoveJobPosition with jobPositionId={jobPositionId}. (UserID: {userId})");

            var jobPosition = await _context.JobPositions.Include(x => x.Applications).SingleOrDefaultAsync(x => x.Id == jobPositionId);
           
            if (jobPosition == null)
                throw new Exception($"Job position with id {jobPositionId} not found. (UserID: {userId})");
            if (jobPosition.Applications.Count != 0)
                throw new Exception($"Job position with id {jobPositionId} has Applications. (UserID: {userId})");

            await _jobPositionRepository.RemoveAsync(jobPosition);
        }

        public async Task RemoveJobPositionFromIndexView(string jobPositionId, string userId)
        {
            _logger.LogInformation($"Executing RemoveJobPositionFromIndexView with jobPositionId={jobPositionId}. (UserID: {userId})");

            var jobPosition = await _context.JobPositions.Include(x => x.Applications).SingleOrDefaultAsync(x => x.Id == jobPositionId);
            if (jobPosition == null)
                throw new Exception($"Job position with id {jobPositionId} not found. (UserID: {userId})");
            if (jobPosition.Applications.Count != 0)
                throw new ApplicationException($"This JobPosition has already Applications.");

            await _jobPositionRepository.RemoveAsync(jobPosition);
        }
    }
}
