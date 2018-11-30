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
using Recruiter.AttributeFilters;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using Recruiter.Models.ApplicationStageViewModels.Shared;
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
        private readonly ICvStorageService _cvStorageService;
        private readonly IApplicationStageService _applicationStageService;

        public ApplicationStageController(IMapper mapper, 
                    ICvStorageService cvStorageService, 
                    IApplicationStageService applicationStageService, 
                    ApplicationDbContext context, 
                    UserManager<ApplicationUser> userManager)
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
        [Route("{stageName?}")]
        public IActionResult ApplicationsStagesToReview(string stageName = "")
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = _applicationStageService.GetViewModelForApplicationsStagesToReview(stageName, myId);
            vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();

            return View(vm);
        }
        #endregion

        #region AssingUserToApplicationStage()
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> AssingUserToApplicationStage(string stageId, 
                                                                        AssingUserToStageViewModel addResponsibleUserToStageViewModel, 
                                                                        string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
                if (users.Count() != 0)
                    ViewData["UsersToAssingToStage"] = new SelectList(users, "Id", "Email");
                return View(addResponsibleUserToStageViewModel);
            }

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.UpdateResponsibleUserInApplicationStage(addResponsibleUserToStageViewModel, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                if (returnUrl != null)
                    return RedirectToLocal(returnUrl);
                else
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
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
                    TempData["Error"] = $"Couldn't process stage: Unknown Application Stage with ID:{stageId}.";
                    if (returnUrl!=null)
                        return RedirectToLocal(returnUrl);
                    else
                        return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
            }
        }
        #endregion

        #region ApplicationApproval
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessApplicationApproval(string stageId, 
                                                                    ProcessApplicationApprovalViewModel applicationApprovalViewModel, 
                                                                    bool accepted = false, 
                                                                    string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.UpdateApplicationApprovalStage(applicationApprovalViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "ApplicationApproval" });
        }
        #endregion

        #region PhoneCall
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessPhoneCall(string stageId, 
                                                            ProcessPhoneCallViewModel phoneCallViewModel, 
                                                            bool accepted = false, 
                                                            string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.UpdatePhoneCallStage(phoneCallViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                if (returnUrl != null)
                    return RedirectToLocal(returnUrl);
                else
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
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
            catch (NotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> AddHomeworkSpecification(string stageId, 
                                                                    AddHomeworkSpecificationViewModel addHomeworkSpecificationViewModel, 
                                                                    string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;

                var vm = await _applicationStageService.GetViewModelForAddHomeworkSpecification(stageId, myId);
                return View(vm);
            }

            try
            {
                await _applicationStageService.UpdateHomeworkSpecification(addHomeworkSpecificationViewModel, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
        }

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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessHomeworkStage(string stageId, 
                                                                ProcessHomeworkStageViewModel processHomeworkStageViewModel, 
                                                                bool accepted = false, 
                                                                string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;

                var vm = await _applicationStageService.GetViewModelForProcessHomeworkStage(stageId, myId);
                return View(vm);
            }

            try
            {
                await _applicationStageService.UpdateHomeworkStage(processHomeworkStageViewModel, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ApplicationStageController.ProcessStage), new { stageId, returnUrl });
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Homework" });
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                if (returnUrl != null)
                    return RedirectToLocal(returnUrl);
                else
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        [HttpPost]
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
                ViewData["ReturnUrl"] = returnUrl;

                var vm = await _applicationStageService.GetViewModelForSetAppointmentsToInterview(setAppointmentsToInterviewViewModel.NewInterviewAppointment.InterviewId, myId);
                vm.NewInterviewAppointment = setAppointmentsToInterviewViewModel.NewInterviewAppointment;
                return View(vm);
            }

            try
            {
                await _applicationStageService.AddNewInterviewAppointments(setAppointmentsToInterviewViewModel, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(ApplicationStageController.ProcessInterview), new { stageId, returnUrl });
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> SendInterviewAppointmentsToConfirm(string stageId, bool accepted = true, string returnUrl = null)
        {
            //ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationStageService.SendInterviewAppointmentsToConfirm(stageId, accepted, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
                //return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

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
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrl != null)
                return RedirectToLocal(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessInterviewStage(string stageId, ProcessInterviewViewModel interviewViewModel, bool accepted = false, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateInterviewStage(interviewViewModel, accepted, myId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "Interview" });
        }

        #endregion

        #region ShowApplicationStageDetails()
        [Route("{stageId?}")]
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

        [Route("{stageId?}")]
        public async Task<IActionResult> ApplicationStageBaseDatails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithIncludeNoTracking(stageId, myId);
            
            return View(stage);
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> HomeworkStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithIncludeNoTracking(stageId, myId) as Homework;
            
            return View(stage);
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> InterviewStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithIncludeNoTracking(stageId, myId) as Interview;

            stage.InterviewAppointments = _context.InterviewAppointments
                                            .Where(x => x.InterviewId == stage.Id)
                                            .OrderBy(x => x.StartTime)
                                            .ToList();

            return View(stage);
        }
        #endregion

        public async Task<IActionResult> ShowAssignedAppointments(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var myAppointments = await _applicationStageService.GetViewModelForShowAssignedAppointments(myId);

            return View(myAppointments);
        }

        [Route("{appointmentId?}")]
        public async Task<IActionResult> RemoveAssignedAppointment(string appointmentId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var appointment = await _applicationStageService.RemoveAssignedAppointment(appointmentId, myId);
            //await _applicationStageService.RemoveAssignedAppointment(appointmentId, myId);

            return RedirectToLocal(returnUrl);
        }

        #region Helpers
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(ApplicationStageController.Index));
        }
        #endregion
    }
}