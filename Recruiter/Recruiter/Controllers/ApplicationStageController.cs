using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.AttributeFilters;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using Recruiter.Models.ApplicationStageViewModels.Shared;
using Recruiter.Models.EmailNotificationViewModel;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
    public class ApplicationStageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ICvStorageService _cvStorageService;
        private readonly IApplicationStageService _applicationStageService;
        private readonly IEmailSender _emailSender;

        public ApplicationStageController(IMapper mapper,
                    ILogger<ApplicationStageController> logger,
                    ICvStorageService cvStorageService, 
                    IApplicationStageService applicationStageService,
                    IEmailSender emailSender,
                    ApplicationDbContext context, 
                    UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _logger = logger;
            _cvStorageService = cvStorageService;
            _applicationStageService = applicationStageService;
            _emailSender = emailSender;
            _context = context;
            _userManager = userManager;
        }

        [Route("/[controller]")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        #region ApplicationsStagesToReview()
        [Route("{stageName?}")]
        public IActionResult ApplicationsStagesToReview(string stageName = "")
        {
            try
            {
                var myId = _userManager.GetUserId(HttpContext.User);
                var vm = _applicationStageService.GetViewModelForApplicationsStagesToReview(stageName, myId);
                vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        #endregion

        #region AssingUserToApplicationStage()
        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> AssingUserToApplicationStage(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var myId = _userManager.GetUserId(HttpContext.User);
                var vm = await _applicationStageService.GetViewModelForAssingUserToStage(stageId, myId);

                var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
                if (users.Count() != 0)
                    ViewData["UsersToAssingToStage"] = new SelectList(users, "Id", "Email");

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToHomeIndex(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> AssingUserToApplicationStage(string stageId, 
                                                                        AssingUserToStageViewModel addResponsibleUserToStageViewModel, 
                                                                        string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ApplicationStageController.AssingUserToApplicationStage), new { stageId, returnUrl });
            }

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.UpdateResponsibleUserInApplicationStage(addResponsibleUserToStageViewModel, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }
            
            if(returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationController.ApplicationDetails), "Application", new { id = addResponsibleUserToStageViewModel.ApplicationId });
        }
        #endregion

        #region ProcessStage()
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessStage(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            ApplicationStageBase stage = null;
            try
            {
                stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
            }

            switch (stage?.GetType().Name)
            {
                case "ApplicationApproval":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessApplicationApproval), new { stageId, returnUrl });
                case "PhoneCall":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessPhoneCall), new { stageId, returnUrl });
                case "Homework":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessHomework), new { stageId, returnUrl });
                case "Interview":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId, returnUrl });
                default:
                    TempData["Error"] = $"Couldn't process stage: Unknown Application Stage type with ID:{stageId}.";
                    return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
            }
        }
        #endregion

        #region ApplicationApproval
        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessApplicationApproval(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForProcessApplicationApproval(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessApplicationApproval(string stageId, 
                                                                    ProcessApplicationApprovalViewModel applicationApprovalViewModel, 
                                                                    bool accepted = false, 
                                                                    string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ApplicationStageController.ProcessApplicationApproval), new { stageId, returnUrl });
            }

            try
            {
                await _applicationStageService.UpdateApplicationApprovalStage(applicationApprovalViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            try
            {
                var stage = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefault(x => x.Id == stageId);
                var callbackUrl = Url.MyApplicationDetailsCallbackLink(stage.Application.Id, Request.Scheme);
                await _emailSender.SendEmailNotificationProcessApplicationApprovalAsync(stage.Application.User.Email, callbackUrl, stage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Email notification has not been sent. StageID:{stageId} (UserID: {myId})");
                _logger.LogError(ex.Message);
                TempData["WarningEmailNotification"] = "Email notification has not been sent.";
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "ApplicationApproval");
        }
        #endregion

        #region PhoneCall
        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessPhoneCall(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForProcessPhoneCall(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessPhoneCall(string stageId, 
                                                            ProcessPhoneCallViewModel phoneCallViewModel, 
                                                            bool accepted = false, 
                                                            string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ApplicationStageController.ProcessPhoneCall), new { stageId, returnUrl });
            }

            try
            {
                await _applicationStageService.UpdatePhoneCallStage(phoneCallViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            try
            {
                var stage = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefault(x => x.Id == stageId);
                var callbackUrl = Url.MyApplicationDetailsCallbackLink(stage.Application.Id, Request.Scheme);
                await _emailSender.SendEmailNotificationProcessPhoneCallAsync(stage.Application.User.Email, callbackUrl, stage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Email notification has not been sent. StageID:{stageId} (UserID: {myId})");
                _logger.LogError(ex.Message);
                TempData["WarningEmailNotification"] = "Email notification has not been sent.";
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "PhoneCall");
            //if (returnUrl != null)
            //    return RedirectToLocal(returnUrl);
            //else
            //    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
        }
        #endregion

        #region Homework
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessHomework(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            Homework stage = null;
            try
            {
                stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId) as Homework;
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
            }

            switch (stage?.HomeworkState) {
                case HomeworkState.WaitingForSpecification:
                    return RedirectToAction(nameof(ApplicationStageController.AddHomeworkSpecification), new { stageId = stage.Id, returnUrl });
                case HomeworkState.WaitingForRead:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
                case HomeworkState.WaitingForSendHomework:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
                case HomeworkState.Completed:
                    return RedirectToAction(nameof(ApplicationStageController.ProcessHomeworkStage), new { stageId = stage.Id, returnUrl });
                default:
                    TempData["Error"] = $"Couldn't process Homework stage: Unknown HomeworkState with ID:{stageId}.";
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
            }
        }

        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> AddHomeworkSpecification(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForAddHomeworkSpecification(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> AddHomeworkSpecification(string stageId, AddHomeworkSpecificationViewModel addHomeworkSpecificationViewModel, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
                return RedirectToAction(nameof(ApplicationStageController.AddHomeworkSpecification), new { stageId, returnUrl });

            try
            {
                await _applicationStageService.UpdateHomeworkSpecification(addHomeworkSpecificationViewModel, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            try
            {
                var stage = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefault(x => x.Id == stageId);
                var callbackUrl = Url.MyApplicationDetailsCallbackLink(stage.Application.Id, Request.Scheme);
                await _emailSender.SendEmailNotificationAddHomeworkSpecificationAsync(stage.Application.User.Email, callbackUrl, stage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Email notification has not been sent. StageID:{stageId} (UserID: {myId})");
                _logger.LogError(ex.Message);
                TempData["WarningEmailNotification"] = "Email notification has not been sent.";
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "Homework");
        }

        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessHomeworkStage(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForProcessHomeworkStage(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessHomeworkStage(string stageId, 
                                                                ProcessHomeworkStageViewModel processHomeworkStageViewModel, 
                                                                bool accepted = false, 
                                                                string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ApplicationStageController.ProcessHomeworkStage), new { stageId, returnUrl });
            }

            try
            {
                await _applicationStageService.UpdateHomeworkStage(processHomeworkStageViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            try
            {
                var stage = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefault(x => x.Id == stageId);
                var callbackUrl = Url.MyApplicationDetailsCallbackLink(stage.Application.Id, Request.Scheme);
                await _emailSender.SendEmailNotificationProcessHomeworkStageAsync(stage.Application.User.Email, callbackUrl, stage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Email notification has not been sent. StageID:{stageId} (UserID: {myId})");
                _logger.LogError(ex.Message);
                TempData["WarningEmailNotification"] = "Email notification has not been sent.";
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "Homework");
        }
        #endregion

        #region Interview
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessInterview(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            Interview stage = null;
            try
            {
                stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId) as Interview;
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
            }

            switch (stage?.InterviewState)
            {
                case InterviewState.WaitingForSettingAppointments:
                    return RedirectToAction(nameof(ApplicationStageController.SetAppointmentsToInterview), new { stageId = stage.Id, returnUrl });
                case InterviewState.RequestForNewAppointments:
                    return RedirectToAction(nameof(ApplicationStageController.SetAppointmentsToInterview), new { stageId = stage.Id, returnUrl });
                case InterviewState.WaitingForConfirmAppointment:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
                case InterviewState.AppointmentConfirmed:
                    return RedirectToAction(nameof(ApplicationStageController.ProcessInterviewStage), new { stageId = stage.Id, returnUrl });
                default:
                    TempData["Error"] = $"Couldn't process Interview stage: Unknown InterviewState with ID:{stageId}.";
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
            }
        }

        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> SetAppointmentsToInterview(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForSetAppointmentsToInterview(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> SetAppointmentsToInterview(string stageId, 
                                                                    SetAppointmentsToInterviewViewModel setAppointmentsToInterviewViewModel,
                                                                    string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (ModelState.IsValid)
            {
                if (setAppointmentsToInterviewViewModel.NewInterviewAppointment.StartTime.ToUniversalTime() < DateTime.UtcNow)
                    ModelState.AddModelError("", "StartTime must be in the future.");
                var collidingAppointments = await _applicationStageService.GetCollidingInterviewAppointment(setAppointmentsToInterviewViewModel.NewInterviewAppointment, myId);
                foreach (var app in collidingAppointments)
                {
                    ModelState.AddModelError("", $"Collision with appointment: " +
                            $"{app.StartTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")} - {app.EndTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")}. " +
                            $"({app.Interview.Application.User.FirstName} {app.Interview.Application.User.LastName} ({app.Interview.Application.User.Email}) - " +
                            $"{app.Interview.Application.JobPosition.Name}) - " +
                            $"{app.InterviewAppointmentState}");
                }
            }
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ApplicationStageController.SetAppointmentsToInterview), new { stageId, returnUrl });
            }

            try
            {
                await _applicationStageService.AddNewInterviewAppointments(setAppointmentsToInterviewViewModel, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId, returnUrl });  //ProcessStage 
            }

            return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId, returnUrl });  //ProcessStage 
        }

        [Route("{appointmentId?}")]
        public async Task<IActionResult> RemoveAppointmentsFromInterview(string appointmentId, string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.RemoveAppointmentsFromInterview(appointmentId, myId);
                TempData["Success"] = "Successfully deleted.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId, returnUrl });
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> SendInterviewAppointmentsToConfirm(string stageId, bool accepted = true, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.SendInterviewAppointmentsToConfirm(stageId, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "Interview");
            }

            try
            {
                var stage = _context.Interviews
                                    .Include(x => x.InterviewAppointments)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefault(x => x.Id == stageId);// as Interview;
                var callbackUrl = Url.MyApplicationDetailsCallbackLink(stage.Application.Id, Request.Scheme);
                
                var interviewAppointmentsToConfirm = new List<InterviewAppointmentToConfirmViewModel>();
                foreach (var appointment in stage.InterviewAppointments.Where(x => x.InterviewAppointmentState == InterviewAppointmentState.WaitingForConfirm))
                {
                    interviewAppointmentsToConfirm.Add(new InterviewAppointmentToConfirmViewModel() {
                        StartTime = appointment.StartTime.ToLocalTime(),
                        EndTime = appointment.EndTime.ToLocalTime(),
                        Duration = appointment.Duration,
                        ConfirmationUrl = Url.ConfirmAppointmentCallbackLink(appointment.Id, Request.Scheme, stage.Id, stage.ApplicationId)
                    });
                }

                await _emailSender.SendEmailNotificationSendInterviewAppointmentsToConfirmAsync(stage.Application.User.Email, callbackUrl, stage, interviewAppointmentsToConfirm);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Email notification has not been sent. StageID:{stageId} (UserID: {myId})");
                _logger.LogError(ex.Message);
                TempData["WarningEmailNotification"] = "Email notification has not been sent.";
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "Interview");
        }

        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessInterviewStage(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForProcessInterviewStage(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessInterviewStage(string stageId, ProcessInterviewViewModel interviewViewModel, bool accepted = false, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ApplicationStageController.ProcessInterviewStage), new { stageId, returnUrl });
            }

            try
            {
                await _applicationStageService.UpdateInterviewStage(interviewViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            try
            {
                var stage = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefault(x => x.Id == stageId);
                var callbackUrl = Url.MyApplicationDetailsCallbackLink(stage.Application.Id, Request.Scheme);
                await _emailSender.SendEmailNotificationProcessInterviewStageAsync(stage.Application.User.Email, callbackUrl, stage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Email notification has not been sent. StageID:{stageId} (UserID: {myId})");
                _logger.LogError(ex.Message);
                TempData["WarningEmailNotification"] = "Email notification has not been sent.";
            }

            return RedirectToLocalOrToApplicationsStagesToReview(returnUrl, "Interview");
        }

        #endregion

        #region ShowApplicationStageDetails()
        [Route("{stageId?}")]
        public async Task<IActionResult> ShowApplicationStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            ApplicationStageBase stage = null;
            try
            {
                stage = await _applicationStageService.GetApplicationStageBase(stageId, myId);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToLocalOrToHomeIndex(returnUrl);
            }

            switch (stage?.GetType().Name)
            {
                case "ApplicationApproval":
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails), 
                                                new { stageId = stage.Id, returnUrl });
                case "PhoneCall":
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails), 
                                                new { stageId = stage.Id, returnUrl });
                case "Homework":
                    return RedirectToAction(nameof(ApplicationStageController.HomeworkStageDetails), 
                                                new { stageId = stage.Id, returnUrl });
                case "Interview":
                    return RedirectToAction(nameof(ApplicationStageController.InterviewStageDetails), 
                                                new { stageId = stage.Id, returnUrl });
                default:
                    if (stage != null)
                    {
                        TempData["Warning"] = $"Couldn't find well known application stage type with ID:{stageId}, below showed primary ApplicationStageBase.";
                        return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails),
                                                    new { stageId = stage.Id, returnUrl });
                    }
                    else
                    {
                        TempData["Error"] = $"Couldn't find application stage details: Unknown stage with ID:{stageId}.";
                        return RedirectToLocalOrToHomeIndex(returnUrl);
                    }
            }
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> ApplicationStageBaseDatails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForApplicationStageBaseDatails(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToHomeIndex(returnUrl);
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> HomeworkStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForHomeworkStageDetails(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToHomeIndex(returnUrl);
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> InterviewStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationStageService.GetViewModelForInterviewStageDetails(stageId, myId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToHomeIndex(returnUrl);
        }
        #endregion

        #region ShowAssignedAppointments()
        public async Task<IActionResult> ShowAssignedAppointments(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var myAppointments = await _applicationStageService.GetViewModelForShowAssignedAppointments(myId);
                return View(myAppointments);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToHomeIndex(returnUrl);
        }
        #endregion

        #region RemoveAssignedAppointment()
        [Route("{appointmentId?}")]
        public async Task<IActionResult> RemoveAssignedAppointment(string appointmentId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.RemoveAssignedAppointment(appointmentId, myId);
                TempData["Success"] = "Appointment successfully removed.";
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToHomeIndex(returnUrl);
        }
        #endregion

        #region Helpers
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.Index));
        }

        private IActionResult RedirectToLocalOrToApplicationsStagesToReview(string returnUrl, string stageName = "")
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName });
        }

        private IActionResult RedirectToLocalOrToHomeIndex(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        #endregion
    }
}