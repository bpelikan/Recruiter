using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recruiter.AttributeFilters;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using Recruiter.Models.ApplicationStageViewModels.Shared;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
    public class ApplicationStageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICvStorageService _cvStorageService;
        private readonly IApplicationStageService _applicationStageService;

        public ApplicationStageController(IMapper mapper, ICvStorageService cvStorageService, IApplicationStageService applicationStageService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _cvStorageService = cvStorageService;
            _applicationStageService = applicationStageService;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        #region ApplicationsStagesToReview()
        public IActionResult ApplicationsStagesToReview(string stageName = "")
        {
            //if (stageName == "Homework")
            //    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReviewHomework), new { stageName = "Homework"});
            
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = _applicationStageService.GetViewModelForApplicationsStagesToReview(stageName, myId);
            vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();

            return View(vm);
        }

        //public IActionResult ApplicationsStagesToReviewHomework(string stageName = "Homework")
        //{
        //    var myId = _userManager.GetUserId(HttpContext.User);
        //    var vm = _applicationStageService.GetViewModelForApplicationsStagesToReview(stageName, myId);
        //    vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();

        //    return View(vm);
        //}
        #endregion

        #region AssingUserToApplicationStage()
        public async Task<IActionResult> AssingUserToApplicationStage(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForAssingUserToStage(stageId, myId);
            
            var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
            if (users.Count() != 0)
                ViewData["UsersToAssingToStage"] = new SelectList(users, "Id", "Email");
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssingUserToApplicationStage(AssingUserToStageViewModel addResponsibleUserToStageViewModel)
        {
            if (!ModelState.IsValid)
                return View(addResponsibleUserToStageViewModel);

            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateResponsibleUserInApplicationStage(addResponsibleUserToStageViewModel, myId);

            return RedirectToAction(nameof(ApplicationController.ApplicationDetails), "Application", new { id = addResponsibleUserToStageViewModel.ApplicationId });
        }
        #endregion

        #region ProcessStage()
        public async Task<IActionResult> ProcessStage(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId);
            
            switch (stage.GetType().Name) {
                case "ApplicationApproval":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessApplicationApproval), new { stageId = stage.Id });
                case "PhoneCall":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessPhoneCall), new { stageId = stage.Id });
                case "Homework":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessHomework), new { stageId = stage.Id });
                case "Interview":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId = stage.Id });
                default:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
            }
        }
        #endregion

        #region ApplicationApproval
        public async Task<IActionResult> ProcessApplicationApproval(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForProcessApplicationApproval(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessApplicationApproval(ProcessApplicationApprovalViewModel applicationApprovalViewModel, bool accepted = false)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateApplicationApprovalStage(applicationApprovalViewModel, accepted, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "ApplicationApproval" });
        }
        #endregion

        #region PhoneCall
        public async Task<IActionResult> ProcessPhoneCall(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForProcessPhoneCall(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPhoneCall(ProcessPhoneCallViewModel phoneCallViewModel, bool accepted = false)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdatePhoneCallStage(phoneCallViewModel, accepted, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
        }
        #endregion

        #region Homework
        public async Task<IActionResult> ProcessHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId) as Homework;

            switch (stage.HomeworkState) {
                case HomeworkState.WaitingForSpecification:
                    return RedirectToAction(nameof(ApplicationStageController.AddHomeworkSpecification), new { stageId = stage.Id });
                case HomeworkState.WaitingForRead:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
                case HomeworkState.WaitingForSendHomework:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
                case HomeworkState.Completed:
                    return RedirectToAction(nameof(ApplicationStageController.ProcessHomeworkStage), new { stageId = stage.Id });
                default:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
            }
        }

        public async Task<IActionResult> AddHomeworkSpecification(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForAddHomeworkSpecification(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddHomeworkSpecification(AddHomeworkSpecificationViewModel addHomeworkSpecificationViewModel)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateHomeworkSpecification(addHomeworkSpecificationViewModel, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
        }

        public async Task<IActionResult> ProcessHomeworkStage(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForProcessHomeworkStage(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessHomeworkStage(ProcessHomeworkStageViewModel processHomeworkStageViewModel, bool accepted = false)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateHomeworkStage(processHomeworkStageViewModel, accepted, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
        }
        #endregion

        #region Interview
        public async Task<IActionResult> ProcessInterview(string stageId)
        {

            //var myId = _userManager.GetUserId(HttpContext.User);
            //var stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId) as Homework;

            //switch (stage.HomeworkState)
            //{
            //    case HomeworkState.WaitingForSpecification:
            //        return RedirectToAction(nameof(ApplicationStageController.AddHomeworkSpecification), new { stageId = stage.Id });
            //    case HomeworkState.WaitingForRead:
            //        return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
            //    case HomeworkState.WaitingForSendHomework:
            //        return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
            //    case HomeworkState.Completed:
            //        return RedirectToAction(nameof(ApplicationStageController.ProcessHomeworkStage), new { stageId = stage.Id });
            //    default:
            //        return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
            //}

            //var myId = _userManager.GetUserId(HttpContext.User);
            //var vm = await _applicationStageService.GetViewModelForProcessInterview(stageId, myId);

            //return View(vm);

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseToProcessStage(stageId, myId) as Interview;

            switch (stage.InterviewState)
            {
                case InterviewState.WaitingForSettingAppointments:
                    return RedirectToAction(nameof(ApplicationStageController.AddAppointmentsToInterview), new { stageId = stage.Id });
                case InterviewState.RequestForNewAppointments:
                    return RedirectToAction(nameof(ApplicationStageController.AddAppointmentsToInterview), new { stageId = stage.Id });
                case InterviewState.WaitingForConfirmAppointment:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
                case InterviewState.AppointmentConfirmed:
                    return RedirectToAction(nameof(ApplicationStageController.ProcessInterviewStage), new { stageId = stage.Id });
                default:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
            }
            //return View(vm);
        }

        #region del
        //[HttpPost]
        //public async Task<IActionResult> ProcessInterview(ProcessInterviewViewModel interviewViewModel, bool accepted = false)
        //{
        //    var myId = _userManager.GetUserId(HttpContext.User);
        //    await _applicationStageService.UpdateInterview(interviewViewModel, accepted, myId);

        //    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
        //}

        //public async Task<IActionResult> AddAppointmentsToInterview(string stageId)
        //{
        //    var myId = _userManager.GetUserId(HttpContext.User);
        //    var vm = await _applicationStageService.GetViewModelForAddAppointmentsToInterview(stageId, myId);

        //    return View(vm);
        //}

        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////
        #endregion

        //[ImportModelState]
        public async Task<IActionResult> AddAppointmentsToInterview(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForAddAppointmentsToInterview(stageId, myId);
            vm.NewInterviewAppointment = new InterviewAppointment()
            {
                Id = Guid.NewGuid().ToString(),
                InterviewId = vm.StageToProcess.Id,
                StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                                            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 00).ToLocalTime()
            };

            return View(vm);
        }

        [HttpPost]
        //[ExportModelState]
        public async Task<IActionResult> AddAppointmentsToInterview(AddAppointmentsToInterviewViewModel addAppointmentsToInterviewViewModel)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (addAppointmentsToInterviewViewModel.NewInterviewAppointment.StartTime < DateTime.UtcNow)
                ModelState.AddModelError("", "StartTime must be in the future.");

            var newInterviewAppointment = new InterviewAppointment()
            {
                Id = addAppointmentsToInterviewViewModel.NewInterviewAppointment.Id,
                InterviewAppointmentState = InterviewAppointmentState.WaitingToAdd,
                InterviewId = addAppointmentsToInterviewViewModel.NewInterviewAppointment.InterviewId,
                StartTime = addAppointmentsToInterviewViewModel.NewInterviewAppointment.StartTime.ToUniversalTime(),
                Duration = addAppointmentsToInterviewViewModel.NewInterviewAppointment.Duration,
                EndTime = addAppointmentsToInterviewViewModel.NewInterviewAppointment.StartTime.ToUniversalTime()
                                        .AddMinutes(addAppointmentsToInterviewViewModel.NewInterviewAppointment.Duration),
            };

            var test = _context.InterviewAppointments
                                .Include(x => x.Interview)
                                    .ThenInclude(x => x.Application).ThenInclude(x => x.User)
                                .Include(x => x.Interview)
                                    .ThenInclude(x => x.Application).ThenInclude(x => x.JobPosition)
                                .Where(x => x.Interview.ResponsibleUserId == myId &&
                                            //(x.InterviewAppointmentState != InterviewAppointmentState.WaitingToAdd ||
                                            //    (x.InterviewAppointmentState == InterviewAppointmentState.WaitingToAdd && x.InterviewId == newInterviewAppointment.InterviewId)) &&
                                            ((newInterviewAppointment.StartTime < x.StartTime && x.StartTime < newInterviewAppointment.EndTime) ||
                                              newInterviewAppointment.StartTime < x.EndTime && x.EndTime < newInterviewAppointment.EndTime) ||
                                              x.StartTime < newInterviewAppointment.StartTime && newInterviewAppointment.EndTime < x.EndTime)
                                .OrderBy(x => x.StartTime);

            foreach (var app in test)
            {
                ModelState.AddModelError("", $"Collision with appointment: {app.StartTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")} - {app.EndTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")}. ({app.Interview.Application.User.FirstName} {app.Interview.Application.User.LastName} ({app.Interview.Application.User.Email}) - {app.Interview.Application.JobPosition.Name})");

            }

            if (!ModelState.IsValid)
            {
                var vm = await _applicationStageService.GetViewModelForAddAppointmentsToInterview(addAppointmentsToInterviewViewModel.NewInterviewAppointment.InterviewId, myId);
                vm.NewInterviewAppointment = addAppointmentsToInterviewViewModel.NewInterviewAppointment;
                return View(vm);
                //return View(addAppointmentsToInterviewViewModel);
            }

            //if (ModelState.ErrorCount != 0)
            //{
            //    var vm = await _applicationStageService.GetViewModelForAddAppointmentsToInterview(addAppointmentsToInterviewViewModel.NewInterviewAppointment.InterviewId, myId);
            //    vm.NewInterviewAppointment = addAppointmentsToInterviewViewModel.NewInterviewAppointment;
            //    return View(vm);
            //}

            

            await _context.InterviewAppointments.AddAsync(newInterviewAppointment);
            await _context.SaveChangesAsync();

            //return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId = newInterviewAppointment.InterviewId });

            return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), 
                                        new { stageId = addAppointmentsToInterviewViewModel.NewInterviewAppointment.InterviewId });
        }

        public async Task<IActionResult> RemoveAppointmentsFromInterview(string appointmentId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            var appointment = await _context.InterviewAppointments
                                            .FirstOrDefaultAsync(x => x.Id == appointmentId);
            if(appointment == null)
                throw new Exception($"InterviewAppointment with id {appointmentId} not found. (UserID: {myId})");
            if (appointment.InterviewAppointmentState == InterviewAppointmentState.WaitingToAdd)
            {
                _context.InterviewAppointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ApplicationStageController.ProcessInterview),
                                        new { stageId = appointment.InterviewId });
            //await _applicationStageService.AddAppointmentsToInterview(addAppointmentsToInterviewViewModel, accepted, myId);

            //return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
        }

        public async Task<IActionResult> AcceptAppointmentsToInterview(string stageId, bool accepted = true)
        {
            var addAppointmentsToInterviewViewModel = new AddAppointmentsToInterviewViewModel()
            {
                StageToProcess = new AddAppointmentsViewModel()
                {
                    Id = stageId,
                }
            };

            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.AddAppointmentsToInterview(addAppointmentsToInterviewViewModel, accepted, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
        }

        #region del
        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////

        //public async Task<IActionResult> AddInterviewAppointments(string stageId)
        //{
        //    var myId = _userManager.GetUserId(HttpContext.User);
        //    var stage = await _context.ApplicationStages
        //                            .Include(x => x.Application)
        //                            .FirstOrDefaultAsync(x => x.Id == stageId);

        //    if (stage.GetType().Name != "Interview")
        //        throw new Exception($"Stage with id {stageId} is not Interview stage. (UserID: {myId})");

        //    var vm = new InterviewAppointment()
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        InterviewId = stage.Id
        //    };

        //    return View(vm);
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddInterviewAppointments(InterviewAppointment interviewAppointment)
        //{
        //    if (!ModelState.IsValid)
        //        return View(interviewAppointment);

        //    var myId = _userManager.GetUserId(HttpContext.User);

        //    var newInterviewAppointment = new InterviewAppointment()
        //    {
        //        Id = interviewAppointment.Id,
        //        InterviewAppointmentState = InterviewAppointmentState.WaitingToAdd,
        //        InterviewId = interviewAppointment.InterviewId,
        //        StartTime = interviewAppointment.StartTime.ToUniversalTime(),
        //        Duration = interviewAppointment.Duration,
        //        EndTime = interviewAppointment.StartTime.ToUniversalTime().AddMinutes(interviewAppointment.Duration),
        //    };

        //    await _context.InterviewAppointments.AddAsync(newInterviewAppointment);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId = newInterviewAppointment.InterviewId });
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddInterviewAppointmentsTest(AddAppointmentsToInterviewViewModel addAppointmentsToInterviewViewModel)
        //{
        //    if (!ModelState.IsValid)
        //        return View(addAppointmentsToInterviewViewModel);

        //    var myId = _userManager.GetUserId(HttpContext.User);

        //    var newInterviewAppointment = new InterviewAppointment()
        //    {
        //        Id = interviewAppointment.Id,
        //        InterviewAppointmentState = InterviewAppointmentState.WaitingToAdd,
        //        InterviewId = interviewAppointment.InterviewId,
        //        StartTime = interviewAppointment.StartTime.ToUniversalTime(),
        //        Duration = interviewAppointment.Duration,
        //        EndTime = interviewAppointment.StartTime.ToUniversalTime().AddMinutes(interviewAppointment.Duration),
        //    };

        //    await _context.InterviewAppointments.AddAsync(newInterviewAppointment);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId = newInterviewAppointment.InterviewId });
        //}


        //[HttpPost]
        //public async Task<IActionResult> AddAppointmentsToInterview(AddAppointmentsToInterviewViewModel addAppointmentsToInterviewViewModel, bool accepted = true)
        //{
        //    var myId = _userManager.GetUserId(HttpContext.User);
        //    await _applicationStageService.AddAppointmentsToInterview(addAppointmentsToInterviewViewModel, accepted, myId);

        //    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
        //}
        #endregion

        public async Task<IActionResult> ProcessInterviewStage(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForProcessInterviewStage(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessInterviewStage(ProcessInterviewViewModel interviewViewModel, bool accepted = false)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateInterviewStage(interviewViewModel, accepted, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
        }

        #endregion

        #region ShowApplicationStageDetails()
        public async Task<IActionResult> ShowApplicationStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBase(stageId, myId);

            switch (stage.GetType().Name)
            {
                case "ApplicationApproval":
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails), 
                                                new { stageId = stage.Id, returnUrl = ViewData["ReturnUrl"] });
                case "PhoneCall":
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails), 
                                                new { stageId = stage.Id, returnUrl = ViewData["ReturnUrl"] });
                case "Homework":
                    return RedirectToAction(nameof(ApplicationStageController.HomeworkStageDetails), 
                                                new { stageId = stage.Id, returnUrl = ViewData["ReturnUrl"] });
                case "Interview":
                    return RedirectToAction(nameof(ApplicationStageController.InterviewStageDetails), 
                                                new { stageId = stage.Id, returnUrl = ViewData["ReturnUrl"] });
                default:
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails), 
                                                new { stageId = stage.Id, returnUrl = ViewData["ReturnUrl"] });
            }
        }

        public async Task<IActionResult> ApplicationStageBaseDatails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithIncludeNoTracking(stageId, myId);
            
            return View(stage);
        }

        public async Task<IActionResult> HomeworkStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithIncludeNoTracking(stageId, myId) as Homework;
            
            return View(stage);
        }

        public async Task<IActionResult> InterviewStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithIncludeNoTracking(stageId, myId) as Interview;

            stage.InterviewAppointments = _context.InterviewAppointments.Where(x => x.InterviewId == stage.Id).ToList();

            return View(stage);
        }
        #endregion

        public async Task<IActionResult> ShowMyAppointments(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var myAppointments = await _applicationStageService.GetViewModelForShowMyAppointments(myId);

            return View(myAppointments);
        }

    }
}