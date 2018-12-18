using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;
using Recruiter.Models.MyApplicationViewModels.Shared;

namespace Recruiter.Services.Implementation
{
    public class MyApplicationService : IMyApplicationService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ICvStorageService _cvStorageService;
        private readonly IQueueMessageSender _queueMessageSender;
        private readonly IApplicationStageService _applicationStageService;
        private readonly IApplicationsViewHistoriesService _applicationsViewHistoriesService;
        private readonly ApplicationDbContext _context;

        public MyApplicationService(
                        IMapper mapper, 
                        ILogger<MyApplicationService> logger, 
                        ICvStorageService cvStorageService,
                        IQueueMessageSender queueMessageSender,
                        IApplicationStageService applicationStageService,
                        IApplicationsViewHistoriesService applicationsViewHistoriesService,
                        ApplicationDbContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _cvStorageService = cvStorageService;
            _queueMessageSender = queueMessageSender;
            _applicationStageService = applicationStageService;
            _applicationsViewHistoriesService = applicationsViewHistoriesService;
            _context = context;
        }

        public IEnumerable<MyApplicationsViewModel> GetMyApplications(string userId)
        {
            _logger.LogInformation($"Executing GetMyApplications. (UserID: {userId})");

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
            _logger.LogInformation($"Executing GetMyApplicationDetails with applicationId={applicationId}. (UserID: {userId})");

            var application = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Include(x => x.ApplicationStages)
                                        .FirstOrDefault(x => x.Id == applicationId);

            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} not found. (UserID: {userId})");
                throw new NotFoundException($"Application with ID:{applicationId} not found.");
            }
            if (userId != application.UserId)
            {
                _logger.LogError($"User with ID:{userId} aren't owner of application with ID:{applicationId}. (UserID: {userId})");
                throw new PermissionException($"You aren't owner of application with ID:{applicationId}.");
            }

            await _applicationsViewHistoriesService.AddApplicationsViewHistory(applicationId, userId);

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
                                                .OrderBy(x => x.Level).ToList(),
                //ConfirmedInterviewAppointment = _context.InterviewAppointments
                //                                            .Include(x => x.Interview)
                //                                                .ThenInclude(x => x.Application)
                //                                            .Where(x => x.Interview.Application.Id == application.Id).FirstOrDefault()
            };

            var confirmedInterviewAppointment = _context.InterviewAppointments
                                                            .Include(x => x.Interview)
                                                                .ThenInclude(x => x.Application)
                                                            .Where(x => x.Interview.Application.Id == application.Id &&
                                                                        x.InterviewAppointmentState == InterviewAppointmentState.Confirmed)
                                                            .FirstOrDefault();
            if (confirmedInterviewAppointment != null)
            {
                confirmedInterviewAppointment.StartTime = confirmedInterviewAppointment.StartTime.ToLocalTime();
                confirmedInterviewAppointment.EndTime = confirmedInterviewAppointment.EndTime.ToLocalTime();
                vm.ConfirmedInterviewAppointment = confirmedInterviewAppointment;
            }

            return vm;
        }

        public async Task DeleteMyApplication(string applicationId, string userId)
        {
            _logger.LogInformation($"Executing DeleteMyApplication with applicationId={applicationId}. (UserID: {userId})");

            var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == applicationId);
            if (application == null)
            {
                _logger.LogError($"Application with ID:{applicationId} doesn't exist. (UserID: {userId})");
                throw new NotFoundException($"Application with ID:{applicationId} doesn't exist.");
            }
            if (application.UserId != userId)
            {
                _logger.LogError($"User with ID:{userId} aren't owner of application with ID:{application.Id}. (UserID: {userId})");
                throw new PermissionException($"You aren't owner of application with ID:{application.Id}.");
            }

            await _cvStorageService.DeleteCvAsync(application.CvFileName);

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
        }

        public async Task<ApplyApplicationViewModel> GetApplyApplicationViewModel(string jobPositionId, string userId)
        {
            _logger.LogInformation($"Executing GetApplyApplicationViewModel with jobPositionId={jobPositionId}. (UserID: {userId})");

            var offer = await _context.JobPositions.SingleOrDefaultAsync(x => x.Id == jobPositionId);
            if (offer == null)
            {
                _logger.LogError($"JobPosition with ID:{jobPositionId} doesn't exist. (UserID: {userId})");
                throw new NotFoundException($"JobPosition with ID:{jobPositionId} doesn't exist.");
            }

            var vm = new ApplyApplicationViewModel()
            {
                JobPositionId = offer.Id,
                JobPositionName = offer.Name,
            };

            return vm;
        }

        public async Task<Application> ApplyMyApplication(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel, string userId)
        {
            _logger.LogInformation($"Executing ApplyMyApplication. (UserID: {userId})");

            if (cv == null)
            {
                _logger.LogError($"CV file not found. (UserID: {userId})");
                throw new NotFoundException($"CV file not found.");
            }

            using (var stream = cv.OpenReadStream())
            {
                var CvFileName = await _cvStorageService.SaveCvAsync(stream, cv.FileName, userId);
                applyApplicationViewModel.CvFileName = CvFileName;
            }

            if (Path.GetExtension(cv.FileName) != ".pdf")
            {
                _logger.LogWarning($"CV must have .pdf extension. FileName:{cv.FileName} (UserID: {userId})");
                throw new InvalidFileExtensionException($"CV must have .pdf extension.");
            }
            if (applyApplicationViewModel.CvFileName == null)
            {
                _logger.LogError($"CvFileName in applyApplicationViewModel equals NULL. (UserID: {userId})");
                throw new NotFoundException($"Something went wrong during uploading CV.");
            }
            if (await _context.Applications
                                .Where(x => x.UserId == userId && x.JobPositionId == applyApplicationViewModel.JobPositionId).CountAsync() != 0)
            {
                _logger.LogWarning($"User with ID:{userId} already send application to offer with ID:{applyApplicationViewModel.JobPositionId}. (UserID: {userId})");
                throw new InvalidActionException($"You have already sent application to this offer.");
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

            await _applicationStageService.AddRequiredStagesToApplication(application.Id, userId);

            return application;
        }


        public async Task<Homework> GetHomeworkStageToShowInProcessMyHomework(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetHomeworkStageToShowInProcessMyHomework with stageId={stageId}. (UserID: {userId})");

            var stage = await GetHomeworkStageToShow(stageId, userId);
            return stage;
        }

        public async Task<Homework> GetViewModelForBeforeReadMyHomework(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForBeforeReadMyHomework with stageId={stageId}. (UserID: {userId})");

            var stage = await GetHomeworkStageToShow(stageId, userId);
            if (stage.HomeworkState != HomeworkState.WaitingForRead)
            {
                _logger.LogError($"Homework with ID:{stageId} is not in WaitingForRead HomeworkState. (UserID: {userId})");
                throw new InvalidActionException($"Homework with ID:{stageId} is not in WaitingForRead HomeworkState.");
            }

            var vm = new Homework()
            {
                Id = stage.Id,
                Duration = stage.Duration,
                Description = "Description is hidden, clicking ,,Show description\" button will start time counting and show you the content of the homework",
                ApplicationId = stage.ApplicationId,
            };

            return vm;
        }

        public async Task UpdateMyHomeworkAsReaded(string stageId, string userId)
        {
            _logger.LogInformation($"Executing UpdateMyHomeworkAsReaded with stageId={stageId}. (UserID: {userId})");

            var stage = await GetHomeworkStageToProcess(stageId, userId);
            if (stage.HomeworkState != HomeworkState.WaitingForRead)
            {
                _logger.LogError($"Homework with ID:{stageId} is not in WaitingForRead HomeworkState. (UserID: {userId})");
                throw new InvalidActionException($"Homework with ID:{stageId} is not in WaitingForRead HomeworkState.");
            }

            stage.StartTime = DateTime.UtcNow;
            stage.EndTime = stage.StartTime?.AddHours(stage.Duration);
            stage.HomeworkState = HomeworkState.WaitingForSendHomework;

            await _context.SaveChangesAsync();
        }

        public async Task<ReadMyHomeworkViewModel> GetViewModelForReadMyHomework(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForReadMyHomework with stageId={stageId}. (UserID: {userId})");

            var stage = await GetHomeworkStageToShow(stageId, userId);
            if (stage.HomeworkState != HomeworkState.WaitingForSendHomework)
            {
                _logger.LogError($"Homework with ID:{stageId} is not in WaitingForSendHomework HomeworkState. (UserID: {userId})");
                throw new InvalidActionException($"Homework with ID:{stageId} is not in WaitingForSendHomework HomeworkState.");
            }

            var vm = _mapper.Map<Homework, ReadMyHomeworkViewModel>(stage);

            return vm;
        }

        public async Task SendMyHomework(ReadMyHomeworkViewModel homework, string userId)
        {
            _logger.LogInformation($"Executing SendMyHomework. (UserID: {userId})");

            var stage = await GetHomeworkStageToProcess(homework.Id, userId);
            if (stage.HomeworkState != HomeworkState.WaitingForSendHomework)
            {
                _logger.LogError($"Homework with ID:{homework.Id} is not in WaitingForSendHomework HomeworkState. (UserID: {userId})");
                throw new InvalidActionException($"Homework with ID:{homework.Id} is not in WaitingForSendHomework HomeworkState.");
            }

            stage.SendingTime = DateTime.UtcNow;
            stage.Url = homework.Url;
            stage.HomeworkState = HomeworkState.Completed;
            await _context.SaveChangesAsync();
        }

        public async Task<Homework> GetViewModelForShowMyHomework(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForShowMyHomework with stageId={stageId}. (UserID: {userId})");

            var stage = await GetHomeworkStageToShow(stageId, userId);
            if (stage.HomeworkState != HomeworkState.Completed)
            {
                _logger.LogError($"Homework with ID:{stageId} is not in Completed HomeworkState. (UserID: {userId})");
                throw new InvalidActionException($"Homework with ID:{stageId} is not in Completed HomeworkState.");
            }

            return stage;
        }

        private async Task<Homework> GetHomeworkStageToShow(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetHomeworkStageToShow with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            if (stage == null)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException($"ApplicationStage with ID:{stageId} not found.");
            }
            if (stage.Application.User.Id != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to get ApplicationStage with ID:{stageId}. (UserID: {userId})");
                throw new PermissionException($"You are not allowed to get ApplicationStage with ID:{stageId}.");
            }

            stage.StartTime = stage.StartTime?.ToLocalTime();
            stage.EndTime = stage.EndTime?.ToLocalTime();
            stage.SendingTime = stage.SendingTime?.ToLocalTime();

            return stage;
        }

        private async Task<Homework> GetHomeworkStageToProcess(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetHomeworkStageToProcess with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            if (stage == null)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException($"ApplicationStage with ID:{stageId} not found.");
            }
            if (stage.Application.User.Id != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to get ApplicationStage with ID:{stageId}. (UserID: {userId})");
                throw new PermissionException($"You are not allowed to get ApplicationStage with ID:{stageId}.");
            }

            return stage;
        }

        public async Task<Interview> GetViewModelForConfirmInterviewAppointments(string stageId, string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForConfirmAppointmentsInInterview with stageId={stageId}. (UserID: {userId})");

            var stage = await _context.ApplicationStages
                                        .Include(x => x.Application)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == stageId) as Interview;
            if (stage == null)
            {
                _logger.LogError($"ApplicationStage with ID:{stageId} not found. (UserID: {userId})");
                throw new NotFoundException($"ApplicationStage with ID:{stageId} not found.");
            }
            if (stage.Application.UserId != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to get ApplicationStage with ID:{stageId}. (UserID: {userId})");
                throw new PermissionException($"You ares not allowed to get ApplicationStage with ID:{stageId}.");
            }
            if (stage.InterviewState != InterviewState.WaitingForConfirmAppointment)
            {
                _logger.LogError($"Interview with ID:{stageId} is not in WaitingForConfirmAppointment InterviewState. (UserID: {userId})");
                throw new PermissionException($"Interview with ID:{stageId} is not in WaitingForConfirmAppointment InterviewState.");
            }

            var appointments = _context.InterviewAppointments
                                            .Where(x => x.InterviewId == stage.Id &&
                                                        x.InterviewAppointmentState == InterviewAppointmentState.WaitingForConfirm &&
                                                        DateTime.UtcNow <= x.StartTime)
                                            .OrderBy(x => x.StartTime);

            foreach (var appointment in appointments)
            {
                appointment.StartTime = appointment.StartTime.ToLocalTime();
                appointment.EndTime = appointment.EndTime.ToLocalTime();
            }

            stage.InterviewAppointments = appointments.ToList();

            return stage;
            #region del
            //foreach (var appointment in appointments)
            //{
            //    appointment.InterviewAppointmentState = InterviewAppointmentState.WaitingForConfirm;
            //}
            //stage.InterviewState = InterviewState.WaitingForConfirmAppointment;

            //_logger.LogInformation($"Executing GetHomeworkStageToShow with stageId={stageId}. (UserID: {userId})");

            //var stage = await _context.ApplicationStages
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.User)
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.JobPosition)
            //                        .AsNoTracking()
            //                        .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            //if (stage == null)
            //    throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {userId})");
            //if (stage.Application.User.Id != userId)
            //    throw new Exception($"User with ID: {userId} is not allowed to get ApplicationStage with ID: {stageId}.");

            //stage.StartTime = stage.StartTime?.ToLocalTime();
            //stage.EndTime = stage.EndTime?.ToLocalTime();
            //stage.SendingTime = stage.SendingTime?.ToLocalTime();

            //return stage;

            //throw new NotImplementedException();
            #endregion
        }

        public async Task ConfirmAppointmentInInterview(string interviewAppointmentId, string userId)
        {
            _logger.LogInformation($"Executing ConfirmAppointmentsInInterview with interviewAppointmentId={interviewAppointmentId}. (UserID: {userId})");

            var appointmentToConfirm = await _context.InterviewAppointments
                                        .Include(x => x.Interview)
                                            .ThenInclude(x => x.InterviewAppointments)
                                        .Include(x => x.Interview)
                                            .ThenInclude(x => x.Application)
                                                .ThenInclude(x => x.User)
                                        .FirstOrDefaultAsync(x => x.Id == interviewAppointmentId);
            if (appointmentToConfirm == null)
            {
                _logger.LogError($"InterviewAppointment with ID:{interviewAppointmentId} not found. (UserID: {userId})");
                throw new NotFoundException($"InterviewAppointment with ID:{interviewAppointmentId} not found.");
            }
            if (appointmentToConfirm.Interview.Application.UserId != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to confirm appointment with ID:{interviewAppointmentId}. (UserID: {userId})");
                throw new PermissionException($"You are not allowed to confirm appointment with ID:{interviewAppointmentId}.");
            }
            if (appointmentToConfirm.StartTime < DateTime.UtcNow)
            {
                _logger.LogError($"InterviewAppointment with ID:{interviewAppointmentId} have StartTime in the past. (UserID: {userId})");
                throw new InvalidActionException($"You can't confirm appointment that StartTime isn't in the future.");
            }
            if (appointmentToConfirm.Interview.InterviewState != InterviewState.WaitingForConfirmAppointment)
            {
                _logger.LogError($"Couldn't confirm InterviewAppointment with ID:{interviewAppointmentId} in Interview with ID:{appointmentToConfirm.InterviewId} " +
                                 $"that is not in WaitingForConfirmAppointment InterviewState. (UserID: {userId})");
                throw new PermissionException($"Couldn't confirm InterviewAppointment with ID:{interviewAppointmentId} in Interview with ID:{appointmentToConfirm.InterviewId} " +
                                                $"that is not in WaitingForConfirmAppointment InterviewState.");
            }

            appointmentToConfirm.InterviewAppointmentState = InterviewAppointmentState.Confirmed;
            appointmentToConfirm.Interview.InterviewState = InterviewState.AppointmentConfirmed;
            appointmentToConfirm.AcceptedByRecruit = true;
            appointmentToConfirm.AcceptedByRecruitTime = DateTime.UtcNow;

            var appointmentsToDelete = appointmentToConfirm.Interview.InterviewAppointments
                                                .Where(x => x.InterviewAppointmentState != InterviewAppointmentState.Confirmed);
            _context.InterviewAppointments.RemoveRange(appointmentsToDelete);
            await _context.SaveChangesAsync();

            await _queueMessageSender.SendInterviewReminderQueueMessageAsync(appointmentToConfirm.Interview.Application.User.Email, appointmentToConfirm);

            //throw new NotImplementedException();
        }

        public async Task RequestForNewAppointmentsInInterview(string interviewId, string userId)
        {
            _logger.LogInformation($"Executing RequestForNewAppointmentsInInterview with interviewAppointmentId={interviewId}. (UserID: {userId})");

            var interview = await _context.Interviews
                                        .Include(x => x.InterviewAppointments)
                                        .Include(x => x.Application)
                                        .FirstOrDefaultAsync(x => x.Id == interviewId);
            if (interview == null)
            {
                _logger.LogError($"Interview with ID:{interviewId} not found. (UserID: {userId})");
                throw new NotFoundException($"Interview with ID:{interviewId} not found.");
            }
            if (interview.Application.UserId != userId)
            {
                _logger.LogError($"User with ID:{userId} is not allowed to request for new appointments in this interview with ID:{interviewId}. (UserID: {userId})");
                throw new PermissionException($"You are not allowed to request for new appointments in Interview with ID:{interviewId}.");
            }

            foreach (var appointment in interview.InterviewAppointments)
            {
                appointment.InterviewAppointmentState = InterviewAppointmentState.Rejected;
            }
            interview.InterviewState = InterviewState.RequestForNewAppointments;
            await _context.SaveChangesAsync();

            #region del
            //var appointmentToConfirm = await _context.InterviewAppointments
            //                            .Include(x => x.Interview)
            //                                .ThenInclude(x => x.InterviewAppointments)
            //                            .Include(x => x.Interview)
            //                                .ThenInclude(x => x.Application)
            //                            .FirstOrDefaultAsync(x => x.Id == interviewAppointmentId);
            //if (appointmentToConfirm == null)
            //    throw new Exception($"InterviewAppointments with id {interviewAppointmentId} not found. (UserID: {userId})");
            //if (appointmentToConfirm.Interview.Application.UserId != userId)
            //    throw new Exception($"User with ID: {userId} is not allowed to confirm appointment with ID: {interviewAppointmentId}. (UserID: {userId})");

            //appointmentToConfirm.InterviewAppointmentState = InterviewAppointmentState.Confirmed;
            //var appointmentsToDelete = appointmentToConfirm.Interview.InterviewAppointments
            //                                    .Where(x => x.InterviewAppointmentState != InterviewAppointmentState.Confirmed);
            //appointmentToConfirm.Interview.InterviewState = InterviewState.AppointmentConfirmed;
            //_context.InterviewAppointments.RemoveRange(appointmentsToDelete);
            //await _context.SaveChangesAsync();

            //throw new NotImplementedException();
            #endregion
        }

    }
}