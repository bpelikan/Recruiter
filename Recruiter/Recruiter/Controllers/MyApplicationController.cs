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

        [Route("/[controller]")]
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
        public async Task<IActionResult> ProcessMyHomework(string stageId, string applicationId = null, string returnUrl = null)
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
                return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
            }

            switch (stage?.HomeworkState)
            {
                case HomeworkState.WaitingForSpecification:
                    TempData["Error"] = $"Waiting for specification...";
                    return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
                case HomeworkState.WaitingForRead:
                    return RedirectToAction(nameof(MyApplicationController.BeforeReadMyHomework), new { stageId = stage.Id, applicationId, returnUrl });
                case HomeworkState.WaitingForSendHomework:
                    return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = stage.Id, applicationId, returnUrl });
                case HomeworkState.Completed:
                    return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = stage.Id, applicationId, returnUrl });
                default:
                    TempData["Error"] = $"Couldn't process Homework stage: Unknown HomeworkState with ID:{stageId}.";
                    return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
            }
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> BeforeReadMyHomework(string stageId, string applicationId = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _myApplicationService.GetViewModelForBeforeReadMyHomework(stageId, myId);
                return View(vm);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
        }

        [HttpPost]
        [Route("{stageId?}")]
        public async Task<IActionResult> BeforeReadMyHomework(string stageId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _myApplicationService.UpdateMyHomeworkAsReaded(stageId, myId);
                return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId, returnUrl });
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyApplicationController.BeforeReadMyHomework), new { stageId, returnUrl });

        }

        [ImportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ReadMyHomework(string stageId, string applicationId = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _myApplicationService.GetViewModelForReadMyHomework(stageId, myId);
                return View(vm);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
        }

        [HttpPost]
        [ExportModelState]
        [Route("{stageId?}")]
        public async Task<IActionResult> ReadMyHomework(string stageId, ReadMyHomeworkViewModel homework, string applicationId = null, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var myId = _userManager.GetUserId(HttpContext.User);
                try
                {
                    await _myApplicationService.SendMyHomework(homework, myId);
                    TempData["Success"] = "Homework sended successfully.";
                    return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = homework.Id, applicationId, returnUrl });
                }
                catch (CustomException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId, applicationId, returnUrl });
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> ShowMyHomework(string stageId, string applicationId = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var stage = await _myApplicationService.GetViewModelForShowMyHomework(stageId, myId);
                return View(stage);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
        }

        [Route("{stageId?}")]
        public async Task<IActionResult> ConfirmInterviewAppointments(string stageId, string applicationId = null, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _myApplicationService.GetViewModelForConfirmInterviewAppointments(stageId, myId);
                return View(vm);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
            
        }

        [HttpPost]
        [Route("{interviewAppointmentId?}")]
        public async Task<IActionResult> ConfirmInterviewAppointments(string interviewAppointmentId, string stageId = null, string applicationId = null, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            try
            {
                await _myApplicationService.ConfirmAppointmentInInterview(interviewAppointmentId, myId);
                TempData["Success"] = "Confirmed.";
                return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyApplicationController.ConfirmInterviewAppointments), new { stageId, applicationId, returnUrl });

        }

        [HttpPost]
        [Route("{interviewId?}")]
        public async Task<IActionResult> RequestForNewAppointmentsInInterview(string interviewId, string applicationId = null, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _myApplicationService.RequestForNewAppointmentsInInterview(interviewId, myId);
                TempData["Success"] = "Success.";
            }
            catch (CustomException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocalOrToMyApplicationDetails(returnUrl, applicationId);
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
            else if(applicationId == null || applicationId == "")
            {
                return RedirectToAction(nameof(MyApplicationController.MyApplications));
            }
            else
            {
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { applicationId });
            }
        }
        #endregion
    }
}