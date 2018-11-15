using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using Recruiter.Models.ApplicationStageViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public class ApplicationStageService : IApplicationStageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ICvStorageService _cvStorageService;

        public ApplicationStageService(ApplicationDbContext context, 
                                        IMapper mapper, 
                                        ILogger<ApplicationStageService> logger, 
                                        ICvStorageService cvStorageService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cvStorageService = cvStorageService;
        }

        public async Task<bool> UpdateNextApplicationStageState(string applicationId)
        {
            _logger.LogInformation($"Executing UpdateNextApplicationStageState with applicationId={applicationId}");

            var application = await _context.Applications
                                                .Include(x => x.ApplicationStages)
                                                .FirstOrDefaultAsync(x => x.Id == applicationId);
            if (application == null)
                throw new Exception($"Application with id {applicationId} not found.)");

            if (application.ApplicationStages.Count() != 0)
            {
                var nextStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State != ApplicationStageState.Finished).FirstOrDefault();
                var prevStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State == ApplicationStageState.Finished).Last();

                if (nextStage != null && nextStage.State == ApplicationStageState.Waiting)
                {
                    if (prevStage.Accepted && nextStage.ResponsibleUserId != null)
                    {
                        nextStage.State = ApplicationStageState.InProgress;
                    }
                    else if (!prevStage.Accepted)
                    {
                        foreach (var stage in application.ApplicationStages.Where(x => x.State != ApplicationStageState.Finished))
                        {
                            stage.Accepted = false;
                            stage.State = ApplicationStageState.Finished;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> AddRequiredStagesToApplication(string applicationId)
        {
            _logger.LogInformation($"Executing AddRequiredStagesToApplication with applicationId={applicationId}");

            var application = _context.Applications.FirstOrDefault(x => x.Id == applicationId);
            if (application == null)
                throw new Exception($"Application with id: {applicationId} not found.");

            var applicationStagesRequirements = await _context.ApplicationStagesRequirements.FirstOrDefaultAsync(x => x.JobPositionId == application.JobPositionId);
            if (applicationStagesRequirements == null)
                throw new Exception($"Application Stages Requirements with id: {application.JobPositionId} not found.");

            List<ApplicationStageBase> applicationStages = new List<ApplicationStageBase>();
            if (applicationStagesRequirements.IsApplicationApprovalRequired)
            {
                applicationStages.Add(new ApplicationApproval()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForApplicatioApprovalId
                });
            }
            if (applicationStagesRequirements.IsPhoneCallRequired)
            {
                applicationStages.Add(new PhoneCall()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForPhoneCallId
                });
            }
            if (applicationStagesRequirements.IsHomeworkRequired)
            {
                applicationStages.Add(new Homework()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForHomeworkId,
                    HomeworkState = HomeworkState.WaitingForSpecification
                });
            }
            if (applicationStagesRequirements.IsInterviewRequired)
            {
                applicationStages.Add(new Interview()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForInterviewId
                });
            }

            if (applicationStages.OrderBy(x => x.Level).First().ResponsibleUserId != null)
                applicationStages.OrderBy(x => x.Level).First().State = ApplicationStageState.InProgress;

            await _context.ApplicationStages.AddRangeAsync(applicationStages);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<ApplicationStageBase> GetApplicationStageBase(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBase with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
                throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {userId})");

            return stage;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseToProcessStage(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBaseToProcessStage with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBase(stageId, userId);
            if (stage.ResponsibleUserId != userId)
                throw new Exception($"User with ID: {userId} is not allowed to process ApplicationStage with ID: {stage.Id} not found. (UserID: {userId})");

            return stage;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseWithInclude(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBaseWithInclude with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .Include(x => x.AcceptedBy)
                                    .Include(x => x.ResponsibleUser)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
                throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {userId})");

            return stage;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseToShowInProcessStage(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBaseToShowInProcessStage with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.ApplicationStages)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
                throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {userId})");
            if (stage.ResponsibleUserId != userId)
                throw new Exception($"User with ID: {userId} is not responsible user of ApplicationStage with ID: {stage.Id}. (UserID: {userId})");

            return stage;
        }


        public async Task<AssingUserToStageViewModel> GetViewModelForAssingUserToStage(string stageId, string userId)
        {
            var stage = await GetApplicationStageBaseWithInclude(stageId, userId);

            var vm = new AssingUserToStageViewModel()
            {
                ApplicationId = stage.ApplicationId,
                StageId = stage.Id,
            };

            return vm;
        }

        public async Task<ProcessApplicationApprovalViewModel> GetViewModelForProcessApplicationApproval(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForProcessApplicationApproval with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = stage.Application.ApplicationStages;

            var vm = new ProcessApplicationApprovalViewModel()
            {
                Application = new ApplicationViewModel()
                {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = applicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, ApplicationApprovalViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return vm;
        }

        public async Task<ProcessPhoneCallViewModel> GetViewModelForProcessPhoneCall(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForProcessPhoneCall with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = stage.Application.ApplicationStages;

            var vm = new ProcessPhoneCallViewModel()
            {
                Application = new ApplicationViewModel()
                {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = applicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, PhoneCallViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return vm;
        }

        public async Task<AddHomeworkSpecificationViewModel> GetViewModelForAddHomeworkSpecification(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForAddHomeworkSpecification with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = stage.Application.ApplicationStages;

            var vm = new AddHomeworkSpecificationViewModel()
            {
                Application = new ApplicationViewModel()
                {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = applicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, HomeworkSpecificationViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return vm;
        }

        public async Task<ProcessHomeworkStageViewModel> GetViewModelForProcessHomeworkStage(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForProcessHomeworkStage with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = stage.Application.ApplicationStages;

            var vm = new ProcessHomeworkStageViewModel()
            {
                Application = new ApplicationViewModel()
                {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = applicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, HomeworkViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return vm;
        }

        public async Task<ProcessInterviewViewModel> GetViewModelForProcessInterview(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForProcessInterview with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = stage.Application.ApplicationStages;

            var vm = new ProcessInterviewViewModel()
            {
                Application = new ApplicationViewModel()
                {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = applicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, InterviewViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return vm;
        }


        public async Task UpdateApplicationApprovalStage(ProcessApplicationApprovalViewModel applicationApprovalViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdateApplicationApprovalStage. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(applicationApprovalViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
                throw new Exception($"ApplicationStage with id {stage.Id} have not InProgress State. (UserID: {userId})");

            stage.Note = applicationApprovalViewModel.StageToProcess.Note;
            stage.Rate = applicationApprovalViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId);
        }

        public async Task UpdatePhoneCallStage(ProcessPhoneCallViewModel phoneCallViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdatePhoneCallStage. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(phoneCallViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
                throw new Exception($"ApplicationStage with id {stage.Id} have not InProgress State. (UserID: {userId})");

            stage.Note = phoneCallViewModel.StageToProcess.Note;
            stage.Rate = phoneCallViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId);
        }

        public async Task UpdateHomeworkSpecification(AddHomeworkSpecificationViewModel addHomeworkSpecificationViewModel, string userId)
        {
            _logger.LogInformation($"Executing UpdateHomeworkSpecification. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(addHomeworkSpecificationViewModel.StageToProcess.Id, userId) as Homework;
            if (stage.State != ApplicationStageState.InProgress)
                throw new Exception($"ApplicationStage with id {stage.Id} have not InProgress State. (UserID: {userId})");
            if (stage.HomeworkState != HomeworkState.WaitingForSpecification)
                throw new Exception($"Homework ApplicationStage with id {stage.Id} have not WaitingForSpecification HomeworkState. (UserID: {userId})");

            stage.Description = addHomeworkSpecificationViewModel.StageToProcess.Description;
            stage.Duration = addHomeworkSpecificationViewModel.StageToProcess.Duration;
            stage.HomeworkState = HomeworkState.WaitingForRead;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHomeworkStage(ProcessHomeworkStageViewModel processHomeworkStageViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdateHomeworkStage. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(processHomeworkStageViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
                throw new Exception($"ApplicationStage with id {stage.Id} have not InProgress State. (UserID: {userId})");

            stage.Note = processHomeworkStageViewModel.StageToProcess.Note;
            stage.Rate = processHomeworkStageViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId);
        }

        public async Task UpdateInterview(ProcessInterviewViewModel interviewViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdateInterview. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(interviewViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
                throw new Exception($"ApplicationStage with id {stage.Id} have not InProgress State. (UserID: {userId})");

            stage.Note = interviewViewModel.StageToProcess.Note;
            stage.Rate = interviewViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId);
        }

        
    }
}
