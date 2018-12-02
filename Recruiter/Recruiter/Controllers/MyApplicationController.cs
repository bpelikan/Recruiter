using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruiter.AttributeFilters;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;
using Recruiter.Models.MyApplicationViewModels.Shared;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = RoleCollection.Recruit)]
    public class MyApplicationController : Controller
    {
        private readonly IMyApplicationService _myApplicationService;
        private readonly ICvStorageService _cvStorageService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MyApplicationController(
            IMyApplicationService myApplicationService,
            ICvStorageService cvStorageService, 
            IMapper mapper,
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context)
        {
            _myApplicationService = myApplicationService;
            _cvStorageService = cvStorageService;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(MyApplicationController.MyApplications));
        }

        public IActionResult MyApplications()
        {
            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = _myApplicationService.GetMyApplications(userId);

                return View(vm);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Route("{applicationId?}")]
        public async Task<IActionResult> MyApplicationDetails(string applicationId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = await _myApplicationService.GetMyApplicationDetails(applicationId, userId);

                return View(vm);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplications(returnUrl);
        }

        [HttpPost]
        [Route("{applicationId?}")]
        public async Task<IActionResult> DeleteMyApplication(string applicationId, string returnUrl = null)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _myApplicationService.DeleteMyApplication(applicationId, userId);
                TempData["Success"] = "Successfully deleted your application.";
                return RedirectToAction(nameof(MyApplicationController.MyApplications));
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplications(returnUrl);
        }

        [ImportModelState]
        [Route("{jobPositionId?}")]
        public async Task<IActionResult> Apply(string jobPositionId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _myApplicationService.GetApplyApplicationViewModel(jobPositionId, userId);
                return View(vm);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplications(returnUrl);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{jobPositionId?}")]
        public async Task<IActionResult> Apply(string jobPositionId, IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(MyApplicationController.Apply), new { jobPositionId, returnUrl });

            var userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var application = await _myApplicationService.ApplyMyApplication(cv, applyApplicationViewModel, userId);
                TempData["Success"] = "Successfully sended.";
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { applicationId = application.Id });
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyApplicationController.Apply), new { jobPositionId, returnUrl });
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> ProcessMyHomework(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            Homework stage = null;
            try
            {
                stage = await _myApplicationService.GetHomeworkStageToShowInProcessMyHomework(stageId, myId);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToLocalOrToMyApplications(returnUrl);
            }

            switch (stage?.HomeworkState)
            {
                case HomeworkState.WaitingForSpecification:
                    TempData["Error"] = $"Waiting for specification...";
                    return RedirectToLocalOrToMyApplicationDetails(returnUrl, stage?.ApplicationId);
                case HomeworkState.WaitingForRead:
                    return RedirectToAction(nameof(MyApplicationController.BeforeReadMyHomework), new { stageId = stage.Id, returnUrl });
                case HomeworkState.WaitingForSendHomework:
                    return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = stage.Id, returnUrl });
                case HomeworkState.Completed:
                    return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = stage.Id, returnUrl });
                default:
                    TempData["Error"] = $"Couldn't process Homework stage: Unknown HomeworkState with ID:{stageId}.";
                    return RedirectToLocalOrToMyApplicationDetails(returnUrl, stage?.ApplicationId);
            }
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> BeforeReadMyHomework(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _myApplicationService.GetViewModelForBeforeReadMyHomework(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> BeforeReadMyHomework(Homework homework, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.UpdateMyHomeworkAsReaded(homework.Id, myId);

            return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = homework.Id });
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> ReadMyHomework(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetViewModelForReadMyHomework(stageId, myId);

            return View(stage);
        }

        [HttpPost]
        public async Task<IActionResult> SendMyHomework(Homework homework, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.SendMyHomework(homework, myId);

            return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = homework.Id });
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> ShowMyHomework(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetViewModelForShowMyHomework(stageId, myId);

            return View(stage);
        }

        [ImportModelState]
        public async Task<IActionResult> ConfirmInterviewAppointments(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (stageId == null)
                return RedirectToLocal(returnUrl);

            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _myApplicationService.GetViewModelForConfirmInterviewAppointments(stageId, myId);

            return View(vm);
        }

        [ExportModelState]
        public async Task<IActionResult> ConfirmAppointmentInInterview(string interviewAppointmentId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            try
            {
                await _myApplicationService.ConfirmAppointmentInInterview(interviewAppointmentId, myId);
            }
            catch (UserInvalidActionException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            if (!ModelState.IsValid)
            {
                var stageIdWithInterview = await _myApplicationService.GetStageIdThatContainInterviewAppointmentWithId(interviewAppointmentId, myId);
                return RedirectToAction(nameof(MyApplicationController.ConfirmInterviewAppointments), new { stageId = stageIdWithInterview });

                //var vm = await _myApplicationService.GetViewModelForConfirmInterviewAppointments(stageId, myId);
                //return View(vm);
            }

            return RedirectToLocal(returnUrl);
            //return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = "Interview" });
        }

        public async Task<IActionResult> RequestForNewAppointmentsInInterview(string interviewId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.RequestForNewAppointmentsInInterview(interviewId, myId);

            return RedirectToLocal(returnUrl);
            //return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = "Interview" });
        }


        #region Helpers
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(MyApplicationController.Index));
            }
        }
        private IActionResult RedirectToLocalOrToMyApplications(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(MyApplicationController.MyApplications));
            }
        }
        private IActionResult RedirectToLocalOrToHomeIndex(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
        private IActionResult RedirectToLocalOrToMyApplicationDetails(string returnUrl, string applicationId)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { applicationId });
            }
        }
        #endregion
    }
}