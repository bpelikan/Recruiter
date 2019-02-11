using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using Recruiter.Models.ApplicationStageViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Recruiter.Services.Implementation
{
    public class ApplicationStageService : IApplicationStageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ICvStorageService _cvStorageService;
        private readonly IStringLocalizer<ApplicationStageService> _stringLocalizer;

        public ApplicationStageService(ApplicationDbContext context, 
                                        IMapper mapper, 
                                        ILogger<ApplicationStageService> logger, 
                                        ICvStorageService cvStorageService,
                                        IStringLocalizer<ApplicationStageService> stringLocalizer)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cvStorageService = cvStorageService;
            _stringLocalizer = stringLocalizer;
        }
        

        //GET
        public async Task<IEnumerable<InterviewAppointment>> GetCollidingInterviewAppointment(InterviewAppointment interview, string userId)
        {
            _logger.LogInformation($"Executing GetCollidingInterviewAppointment. (UserID: {userId})");

            interview.StartTime = interview.StartTime.ToUniversalTime();
            interview.EndTime = interview.StartTime.ToUniversalTime()
                                        .AddMinutes(interview.Duration);

            var collisionAppointments = await _context.InterviewAppointments
                        .Include(x => x.Interview)
                            .ThenInclude(x => x.Application)
                                .ThenInclude(x => x.User)
                        .Include(x => x.Interview)
                            .ThenInclude(x => x.Application)
                                .ThenInclude(x => x.JobPosition)
                        .Where(x => x.Interview.ResponsibleUserId == userId &&
                                ((x.InterviewAppointmentState != InterviewAppointmentState.Rejected && x.InterviewAppointmentState != InterviewAppointmentState.Finished) ||
                                (x.InterviewAppointmentState == InterviewAppointmentState.Rejected && x.InterviewId == interview.InterviewId)) &&
                                //(x.InterviewAppointmentState != InterviewAppointmentState.WaitingToAdd ||
                                //    (x.InterviewAppointmentState == InterviewAppointmentState.WaitingToAdd && x.InterviewId == newInterviewAppointment.InterviewId)) &&
                                (interview.StartTime <= x.StartTime && x.StartTime < interview.EndTime ||
                                    interview.StartTime < x.EndTime && x.EndTime <= interview.EndTime ||
                                    x.StartTime <= interview.StartTime && interview.EndTime <= x.EndTime))
                        .OrderBy(x => x.StartTime).ToListAsync();

            return collisionAppointments;
        }


        //GET ApplicationStageBase
        public async Task<ApplicationStageBase> GetApplicationStageBase(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBase with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["ApplicationStage with ID:{0} not found.", stageId]);
            }

            return stage;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseToProcessStage(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBaseToProcessStage with stageId={stageId}. (UserID: {userId})");

            //var stage = await GetApplicationStageBase(stageId, userId);
            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["ApplicationStage with ID:{0} not found.", stageId]);
            }
            if (stage.ResponsibleUserId != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to process ApplicationStage with ID:{stage.Id}. (UserID: {userId})");
                throw new PermissionException(_stringLocalizer["You are not allowed to process ApplicationStage with ID:{0}.", stage.Id]);
            }
            if (stage.State != ApplicationStageState.InProgress)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} isn't in InProgress state. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} isn't in InProgress state.", stageId]);
            }

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
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["ApplicationStage with ID:{0} not found.", stageId]);
            }
            if (stage.ResponsibleUserId != userId)
            {
                _logger.LogError($"User with ID: {userId} is not responsible for the ApplicationStage with ID:{stage.Id}. (UserID: {userId})");
                throw new PermissionException(_stringLocalizer["You are not responsible for the ApplicationStage with ID:{0}.", stage.Id]);
            }
            //if (stage.State != ApplicationStageState.InProgress)
            //{
            //    _logger.LogError($"ApplicationStage with ID:{stageId} isn't in InProgress state. (UserID: {userId})");
            //    throw new InvalidActionException($"ApplicationStage with ID:{stageId} isn't in InProgress state.");
            //}

            return stage;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseWithIncludeNoTracking(string stageId, string userId)
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
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["ApplicationStage with ID:{0} not found.", stageId]);
            }

            return stage;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseWithIncludeOtherStages(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetApplicationStageBaseWithIncludeOtherStages with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.ApplicationStages)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .Include(x => x.AcceptedBy)
                                    .Include(x => x.ResponsibleUser)
                                    .FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["ApplicationStage with ID:{0} not found.", stageId]);
            }

            return stage;
        }

        


        //GET ViewModelFor
        public ApplicationsStagesToReviewViewModel GetViewModelForApplicationsStagesToReview(string stageName, string userId)
        {
            List<StagesViewModel> assingStagesCountSortedByName = GetAssignedStagesCountSortedByName(userId);

            var stages = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .Where(x => x.State == ApplicationStageState.InProgress &&
                                                    x.ResponsibleUserId == userId &&
                                                    (x.GetType().Name == stageName || stageName == ""));

            var vm = new ApplicationsStagesToReviewViewModel()
            {
                StageSortedByName = assingStagesCountSortedByName,
            };
            vm.AsignedStages = new List<AsignedStagesViewModel>();

            if (stageName == "Homework")
            {
                foreach (Homework stage in stages)
                {
                    stage.StartTime = stage.StartTime?.ToLocalTime();
                    stage.EndTime = stage.EndTime?.ToLocalTime();
                    stage.SendingTime = stage.SendingTime?.ToLocalTime();

                    vm.AsignedStages.Add(new AsignedStagesViewModel()
                    {
                        Application = new ApplicationViewModel()
                        {
                            Id = stage.Application.Id,
                            CreatedAt = stage.Application.CreatedAt.ToLocalTime(),
                            User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                            JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                        },
                        CurrentStage = stage,
                    });
                }
            }
            else
            {
                foreach (var stage in stages)
                {
                    if (stage.GetType().Name == "Interview")
                    {
                        var interview = stage as Interview;
                        if (interview.InterviewState == InterviewState.AppointmentConfirmed)
                        {
                            interview.InterviewAppointments = _context.InterviewAppointments.Where(x => x.InterviewId == stage.Id).ToList();
                        }
                        if (interview.InterviewAppointments != null)
                        {
                            foreach (var x in interview.InterviewAppointments)
                            {
                                x.StartTime = x.StartTime.ToLocalTime();
                                x.EndTime = x.EndTime.ToLocalTime();
                            }
                        }
                        
                        vm.AsignedStages.Add(new AsignedStagesViewModel()
                        {
                            Application = new ApplicationViewModel()
                            {
                                Id = stage.Application.Id,
                                CreatedAt = stage.Application.CreatedAt.ToLocalTime(),
                                User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                                JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                            },
                            CurrentStage = interview,
                        });
                    }
                    else
                    {
                        vm.AsignedStages.Add(new AsignedStagesViewModel()
                        {
                            Application = new ApplicationViewModel()
                            {
                                Id = stage.Application.Id,
                                CreatedAt = stage.Application.CreatedAt.ToLocalTime(),
                                User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                                JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                            },
                            CurrentStage = stage,
                        });
                    }
                }
            }

            return vm;
        }

        public async Task<AssingUserToStageViewModel> GetViewModelForAssingUserToStage(string stageId, string userId)
        {
            var stage = await GetApplicationStageBaseWithIncludeNoTracking(stageId, userId);

            if (stage.State != ApplicationStageState.Waiting)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} isn't in Waiting state. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} isn't in Waiting state.", stageId]);
            }

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
            var applicationStages = GetStagesFromApplicationId(stage.ApplicationId, userId);

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
            var applicationStages = GetStagesFromApplicationId(stage.ApplicationId, userId);

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
            var applicationStages = GetStagesFromApplicationId(stage.ApplicationId, userId);

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
            var applicationStages = GetStagesFromApplicationId(stage.ApplicationId, userId);

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
            vm.StageToProcess.StartTime = vm.StageToProcess.StartTime?.ToLocalTime();
            vm.StageToProcess.SendingTime = vm.StageToProcess.SendingTime?.ToLocalTime();
            vm.StageToProcess.EndTime = vm.StageToProcess.EndTime?.ToLocalTime();

            return vm;
        }

        public async Task<SetAppointmentsToInterviewViewModel> GetViewModelForSetAppointmentsToInterview(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForAddAppointmentsToInterview with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = GetStagesFromApplicationId(stage.ApplicationId, userId);

            var vm = new SetAppointmentsToInterviewViewModel()
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
                StageToProcess = _mapper.Map<ApplicationStageBase, SetAppointmentsViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            var appointments = _context.InterviewAppointments
                                            .Where(x => x.InterviewId == stage.Id)
                                            .OrderBy(x => x.StartTime);

            foreach (var appointment in appointments)
            {
                appointment.StartTime = appointment.StartTime.ToLocalTime();
                appointment.EndTime = appointment.EndTime.ToLocalTime();
            }
            vm.StageToProcess.InterviewAppointments = appointments.ToList();
            vm.NewInterviewAppointment = new InterviewAppointment()
            {
                Id = Guid.NewGuid().ToString(),
                InterviewId = vm.StageToProcess.Id,
                StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 00).ToLocalTime()
            };

            return vm;
        }

        public async Task<ProcessInterviewViewModel> GetViewModelForProcessInterviewStage(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForProcessInterview with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToShowInProcessStage(stageId, userId);
            var applicationStages = GetStagesFromApplicationId(stage.ApplicationId, userId);

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

            var appointments = _context.InterviewAppointments.Where(x => x.InterviewId == stage.Id);
            vm.StageToProcess.InterviewAppointments = appointments.ToList();
            if (vm.StageToProcess.InterviewAppointments != null)
            {
                foreach (var x in vm.StageToProcess.InterviewAppointments)
                {
                    x.StartTime = x.StartTime.ToLocalTime();
                    x.EndTime = x.EndTime.ToLocalTime();
                }
            }

            return vm;
        }

        public async Task<IEnumerable<InterviewAppointment>> GetViewModelForShowAssignedAppointments(string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForShowAssignedAppointments. (UserID: {userId})");

            var myAppointments = await _context.InterviewAppointments
                .Include(x => x.Interview)
                    .ThenInclude(x => x.Application).ThenInclude(x => x.User)
                .Include(x => x.Interview)
                    .ThenInclude(x => x.Application).ThenInclude(x => x.JobPosition)
                .Where(x => x.Interview.ResponsibleUserId == userId && 
                            x.InterviewAppointmentState != InterviewAppointmentState.Finished) //&&
                            //DateTime.UtcNow <= x.StartTime)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            foreach (var myAppointment in myAppointments)
            {
                myAppointment.StartTime = myAppointment.StartTime.ToLocalTime();
                myAppointment.EndTime = myAppointment.EndTime.ToLocalTime();
                myAppointment.AcceptedByRecruitTime = myAppointment.AcceptedByRecruitTime?.ToLocalTime();
            }
            return myAppointments;
        }

        public async Task<ApplicationStageBase> GetViewModelForApplicationStageBaseDatails(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForApplicationStageBaseDatails with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseWithIncludeNoTracking(stageId, userId);
            return stage;
        }

        public async Task<Homework> GetViewModelForHomeworkStageDetails(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForInterviewStageDetails with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseWithIncludeNoTracking(stageId, userId) as Homework;
            return stage;
        }

        public async Task<Interview> GetViewModelForInterviewStageDetails(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForInterviewStageDetails with stageId={stageId}. (UserID: {userId})");

            var stage = await GetApplicationStageBaseWithIncludeNoTracking(stageId, userId) as Interview;
            stage.InterviewAppointments = _context.InterviewAppointments
                                            .Where(x => x.InterviewId == stage.Id)
                                            .OrderBy(x => x.StartTime)
                                            .ToList();
            return stage;
        }

        //ADD
        public async Task<bool> AddRequiredStagesToApplication(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing AddRequiredStagesToApplication with applicationId={applicationId}");

            var application = _context.Applications.FirstOrDefault(x => x.Id == applicationId);
            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["Application with ID:{0} not found.", applicationId]);
            }

            var applicationStagesRequirements = await _context.ApplicationStagesRequirements.FirstOrDefaultAsync(x => x.JobPositionId == application.JobPositionId);
            if (applicationStagesRequirements == null)
            {
                _logger.LogError($"Application Stages Requirements with ID:{application.JobPositionId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["Application Stages Requirements with ID:{0} not found.", application.JobPositionId]);
            }

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

            if (applicationStages.Count() != 0 && applicationStages.OrderBy(x => x.Level).First().ResponsibleUserId != null)
                applicationStages.OrderBy(x => x.Level).First().State = ApplicationStageState.InProgress;

            await _context.ApplicationStages.AddRangeAsync(applicationStages);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task AddNewInterviewAppointments(InterviewAppointment newInterviewAppointment, string userId)
        {
            _logger.LogInformation($"Executing AddNewInterviewAppointments. (UserID: {userId})");

            newInterviewAppointment.InterviewAppointmentState = InterviewAppointmentState.WaitingToAdd;
            newInterviewAppointment.StartTime = newInterviewAppointment.StartTime.ToUniversalTime();
            newInterviewAppointment.EndTime = newInterviewAppointment.StartTime.ToUniversalTime().AddMinutes(newInterviewAppointment.Duration);

            await _context.InterviewAppointments.AddAsync(newInterviewAppointment);
            await _context.SaveChangesAsync();
        }

        //public async Task AddNewInterviewAppointments(SetAppointmentsToInterviewViewModel setAppointmentsToInterviewViewModel, string userId)
        //{
        //    _logger.LogInformation($"Executing AddNewInterviewAppointments. (UserID: {userId})");

        //    var newInterviewAppointment = new InterviewAppointment()
        //    {
        //        Id = setAppointmentsToInterviewViewModel.NewInterviewAppointment.Id,
        //        InterviewAppointmentState = InterviewAppointmentState.WaitingToAdd,
        //        InterviewId = setAppointmentsToInterviewViewModel.NewInterviewAppointment.InterviewId,
        //        StartTime = setAppointmentsToInterviewViewModel.NewInterviewAppointment.StartTime.ToUniversalTime(),
        //        Duration = setAppointmentsToInterviewViewModel.NewInterviewAppointment.Duration,
        //        EndTime = setAppointmentsToInterviewViewModel.NewInterviewAppointment.StartTime.ToUniversalTime()
        //                                .AddMinutes(setAppointmentsToInterviewViewModel.NewInterviewAppointment.Duration),
        //    };

        //    await _context.InterviewAppointments.AddAsync(newInterviewAppointment);
        //    await _context.SaveChangesAsync();
        //}


        //UPDATE
        public async Task<bool> UpdateNextApplicationStageState(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing UpdateNextApplicationStageState with applicationId={applicationId}. (UserID: {userId})");

            var application = await _context.Applications
                                                .Include(x => x.ApplicationStages)
                                                .FirstOrDefaultAsync(x => x.Id == applicationId);
            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["Application with ID:{0} not found.", applicationId]);
            }

            if (application.ApplicationStages.Count() != 0)
            {
                var nextStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State != ApplicationStageState.Finished).FirstOrDefault();
                var prevStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State == ApplicationStageState.Finished).LastOrDefault();

                if (nextStage != null && nextStage.State == ApplicationStageState.Waiting)
                {
                    if ((prevStage == null || prevStage.Accepted) && nextStage.ResponsibleUserId != null)
                    {
                        nextStage.State = ApplicationStageState.InProgress;
                    }
                    else if (prevStage != null && !prevStage.Accepted)
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

        public async Task UpdateResponsibleUserInApplicationStage(AssingUserToStageViewModel addResponsibleUserToStageViewModel, string userId)
        {
            _logger.LogInformation($"Executing UpdateResponsibleUserInApplicationStage. (UserID: {userId})");

            var stage = await GetApplicationStageBaseWithIncludeOtherStages(addResponsibleUserToStageViewModel.StageId, userId);
            if (stage.State != ApplicationStageState.Waiting && stage.ResponsibleUserId != null)
            {
                _logger.LogError($"Can't change ResponsibleUser in ApplicationStage with ID:{stage.Id} this is possible only in Waiting state. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["Can't change ResponsibleUser in ApplicationStage with ID:{0} this is possible only in Waiting state.", stage.Id]);
            }

            stage.ResponsibleUserId = addResponsibleUserToStageViewModel.UserId;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId, userId);
        }

        public async Task UpdateApplicationApprovalStage(ProcessApplicationApprovalViewModel applicationApprovalViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdateApplicationApprovalStage. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(applicationApprovalViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
            {
                _logger.LogError($"ApplicationStage with ID:{stage.Id} have not InProgress State. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} have not InProgress State.", stage.Id]);
            }

            stage.Note = applicationApprovalViewModel.StageToProcess.Note;
            stage.Rate = applicationApprovalViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId, userId);
        }

        public async Task UpdatePhoneCallStage(ProcessPhoneCallViewModel phoneCallViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdatePhoneCallStage. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(phoneCallViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
            {
                _logger.LogError($"ApplicationStage with ID:{stage.Id} have not InProgress State. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} have not InProgress State.", stage.Id]);
            }

            stage.Note = phoneCallViewModel.StageToProcess.Note;
            stage.Rate = phoneCallViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId, userId);
        }

        public async Task UpdateHomeworkSpecification(AddHomeworkSpecificationViewModel addHomeworkSpecificationViewModel, string userId)
        {
            _logger.LogInformation($"Executing UpdateHomeworkSpecification. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(addHomeworkSpecificationViewModel.StageToProcess.Id, userId) as Homework;
            //if (stage.State != ApplicationStageState.InProgress)
            //{
            //    _logger.LogError($"ApplicationStage with ID:{stage.Id} isn't in InProgress State. (UserID: {userId})");
            //    throw new InvalidActionException($"ApplicationStage with ID:{stage.Id} isn't in InProgress State.");
            //}
            if (stage.HomeworkState != HomeworkState.WaitingForSpecification)
            {
                _logger.LogError($"Homework ApplicationStage with ID:{stage.Id} have not WaitingForSpecification HomeworkState. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} already has a homework.", stage.Id]);
            }

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
            {
                _logger.LogError($"ApplicationStage with ID:{stage.Id} isn't in InProgress State. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} have not InProgress State.", stage.Id]);
            }

            stage.Note = processHomeworkStageViewModel.StageToProcess.Note;
            stage.Rate = processHomeworkStageViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId, userId);
        }

        public async Task UpdateInterviewStage(ProcessInterviewViewModel interviewViewModel, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing UpdateInterview. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(interviewViewModel.StageToProcess.Id, userId);
            if (stage.State != ApplicationStageState.InProgress)
            {
                _logger.LogError($"ApplicationStage with ID:{stage.Id} isn't in InProgress State.. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} have not InProgress State.", stage.Id]);
            }

            var appointments = _context.InterviewAppointments.Where(x => x.InterviewId == stage.Id);
            foreach (var appointment in appointments)
                appointment.InterviewAppointmentState = InterviewAppointmentState.Finished;

            stage.Note = interviewViewModel.StageToProcess.Note;
            stage.Rate = interviewViewModel.StageToProcess.Rate;
            stage.Accepted = accepted;
            stage.AcceptedById = userId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await UpdateNextApplicationStageState(stage.ApplicationId, userId);
        }


        //REMOVE
        public async Task RemoveAssignedAppointment(string appointmentId, string userId)
        {
            _logger.LogInformation($"Executing RemoveAppointmentsAssignToMe with appointmentId={appointmentId}. (UserID: {userId})");

            var appointment = await _context.InterviewAppointments
                                            .Include(x => x.Interview)
                                            .FirstOrDefaultAsync(x => x.Id == appointmentId);
            if (appointment == null)
            {
                _logger.LogError($"InterviewAppointment with ID:{appointmentId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["InterviewAppointment with ID:{0} not found.", appointmentId]);
            }
            if (appointment.Interview.ResponsibleUserId != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to delete InterviewAppointment with ID:{appointmentId}. (UserID: {userId})");
                throw new PermissionException(_stringLocalizer["You are not allowed to delete InterviewAppointment with ID:{0}.", appointmentId]);
            }

            if (appointment.InterviewAppointmentState != InterviewAppointmentState.WaitingToAdd)
            {
                _logger.LogError($"InterviewAppointment with ID:{appointmentId} isn't in WaitingToAdd state. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["InterviewAppointment with ID:{0} isn't in WaitingToAdd state.", appointmentId]);
            }

            _context.InterviewAppointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAppointmentsFromInterview(string appointmentId, string userId)
        {
            _logger.LogInformation($"Executing RemoveAppointmentsFromInterview with appointmentId={appointmentId}. (UserID: {userId})");

            var appointment = await _context.InterviewAppointments
                                            .FirstOrDefaultAsync(x => x.Id == appointmentId);
            if (appointment == null)
            {
                _logger.LogError($"InterviewAppointment with ID:{appointmentId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["InterviewAppointment with ID:{0} not found.", appointmentId]);
            }
            if (appointment.InterviewAppointmentState != InterviewAppointmentState.WaitingToAdd)
            {
                _logger.LogError($"Can't remove InterviewAppointment with ID:{appointmentId} this is possible only in WaitingToAdd state. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["Can't remove InterviewAppointment with ID:{0} this is possible only in WaitingToAdd state.", appointmentId]);
            }

            _context.InterviewAppointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
        

        //SEND
        public async Task SendInterviewAppointmentsToConfirm(string stageId, bool accepted, string userId)
        {
            _logger.LogInformation($"Executing AddAppointmentsToInterview. (UserID: {userId})");

            var stage = await GetApplicationStageBaseToProcessStage(stageId, userId) as Interview;
            if (stage.ResponsibleUserId != userId)
            {
                _logger.LogError($"User with ID:{userId} is not responsible user of ApplicationStage with ID: {stage.Id}. (UserID: {userId})");
                throw new PermissionException(_stringLocalizer["You are not responsible user of ApplicationStage with ID: {0}.", stage.Id]);
            }
            if (stage.State != ApplicationStageState.InProgress)
            {
                _logger.LogError($"ApplicationStage with ID:{stage.Id} isn't in InProgress State. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["ApplicationStage with ID:{0} isn't in InProgress state.", stage.Id]);
            }
            if (stage.InterviewState != InterviewState.WaitingForSettingAppointments && stage.InterviewState != InterviewState.RequestForNewAppointments)
            {
                _logger.LogError($"Message. (UserID: {userId})");
                throw new InvalidActionException(_stringLocalizer["Interview ApplicationStage with ID:{0} isn't in WaitingForSettingAppointments or RequestForNewAppointments InterviewState.", stage.Id]);
            }

            var appointments = _context.InterviewAppointments
                                            .Where(x => x.InterviewId == stage.Id &&
                                                        x.InterviewAppointmentState == InterviewAppointmentState.WaitingToAdd);
            if (appointments.Count() != 0)
            {
                if (accepted)
                {
                    foreach (var appointment in appointments)
                    {
                        appointment.InterviewAppointmentState = InterviewAppointmentState.WaitingForConfirm;
                    }
                    stage.InterviewState = InterviewState.WaitingForConfirmAppointment;
                }
                else
                {
                    _context.InterviewAppointments.RemoveRange(appointments);
                }

                await _context.SaveChangesAsync();
            }


            //var stage = await GetApplicationStageBaseToProcessStage(addHomeworkSpecificationViewModel.StageToProcess.Id, userId) as Homework;
            //if (stage.State != ApplicationStageState.InProgress)
            //    throw new Exception($"ApplicationStage with id {stage.Id} have not InProgress State. (UserID: {userId})");
            //if (stage.HomeworkState != HomeworkState.WaitingForSpecification)
            //    throw new Exception($"Homework ApplicationStage with id {stage.Id} have not WaitingForSpecification HomeworkState. (UserID: {userId})");

            //stage.Description = addHomeworkSpecificationViewModel.StageToProcess.Description;
            //stage.Duration = addHomeworkSpecificationViewModel.StageToProcess.Duration;
            //stage.HomeworkState = HomeworkState.WaitingForRead;
            //await _context.SaveChangesAsync();
        }

        
        //PRIVATE
        private IQueryable<ApplicationStageBase> GetStagesFromApplicationId(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing GetStagesFromApplicationId with applicationId={applicationId}. (UserID: {userId})");
            var applicationStages = _context.ApplicationStages
                        .Include(x => x.AcceptedBy)
                        .Include(x => x.ResponsibleUser)
                        .Where(x => x.ApplicationId == applicationId)
                        .AsNoTracking();
            if (applicationStages == null || applicationStages.Count() == 0)
            {
                _logger.LogError($"ApplicationStages in application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException(_stringLocalizer["ApplicationStages in application with ID:{0} not found.", applicationId]);
            }

            return applicationStages;
        }

        private List<StagesViewModel> GetAssignedStagesCountSortedByName(string userId)
        {
            _logger.LogInformation($"Executing GetAssignedStagesCountSortedByName with userId={userId}. (UserID: {userId})");

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                stagesSortedByName.Add(new StagesViewModel()
                {
                    Name = t.Name,
                    Quantity = _context.ApplicationStages
                                            .AsNoTracking()
                                            .Where(x => x.State == ApplicationStageState.InProgress &&
                                                        x.ResponsibleUserId == userId &&
                                                        x.GetType().Name == t.Name).Count(),
                });
            }

            return stagesSortedByName;
        }

        
    }
}
