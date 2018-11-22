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
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;
using Recruiter.Models.MyApplicationViewModels.Shared;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Authorize(Roles = RoleCollection.Recruit)]
    public class MyApplicationController : Controller
    {
        private readonly IMyApplicationService _myApplicationService;
        private readonly ICvStorageService _cvStorageService;
        private readonly IMapper _mapper;
        //private readonly IApplicationStageService _applicationStageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MyApplicationController(
            IMyApplicationService myApplicationService,
            ICvStorageService cvStorageService, 
            IMapper mapper,
            //IApplicationStageService applicationStageService,
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context)
        {
            _myApplicationService = myApplicationService;
            _cvStorageService = cvStorageService;
            _mapper = mapper;
            //_applicationStageService = applicationStageService;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            //return View();
            return RedirectToAction(nameof(MyApplicationController.MyApplications));
        }

        //[Authorize(Roles = RoleCollection.Recruit)]
        public IActionResult MyApplications()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = _myApplicationService.GetMyApplications(userId);

            return View(vm);
        }

        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<ActionResult> MyApplicationDetails(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = await _myApplicationService.GetMyApplicationDetails(id, userId);

            return View(vm);
        }

        [HttpPost]
        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<IActionResult> DeleteMyApplication(string id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.DeleteMyApplication(id, userId);

            return RedirectToAction(nameof(MyApplicationController.MyApplications));
        }

        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<IActionResult> Apply(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = await _myApplicationService.GetApplyApplicationViewModel(id, userId);

            return View(vm);
        }

        [HttpPost]
        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<IActionResult> Apply(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel)
        {
            if (!ModelState.IsValid)
                return View(applyApplicationViewModel);
           
            var userId = _userManager.GetUserId(HttpContext.User);

            try
            {
                var application = await _myApplicationService.ApplyMyApplication(cv, applyApplicationViewModel, userId);
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = application.Id });
            }
            catch (ApplicationException applicationException)
            {
                ModelState.AddModelError("", applicationException.Message);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong, please try again.");
            }

            return View(applyApplicationViewModel);
        }

        public async Task<IActionResult> ProcessMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetHomeworkStageToShowInProcessMyHomework(stageId, myId);

            switch (stage.HomeworkState)
            {
                case HomeworkState.WaitingForRead:
                    return RedirectToAction(nameof(MyApplicationController.BeforeReadMyHomework), new { stageId = stage.Id });
                case HomeworkState.WaitingForSendHomework:
                    return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = stage.Id });
                case HomeworkState.Completed:
                    return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = stage.Id });
                default:
                    return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
            }
        }

        public async Task<IActionResult> BeforeReadMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _myApplicationService.GetViewModelForBeforeReadMyHomework(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> BeforeReadMyHomework(Homework homework)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.UpdateMyHomeworkAsReaded(homework.Id, myId);

            return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = homework.Id });
        }

        public async Task<IActionResult> ReadMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetViewModelForReadMyHomework(stageId, myId);

            return View(stage);
        }

        [HttpPost]
        public async Task<IActionResult> SendMyHomework(Homework homework)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.SendMyHomework(homework, myId);

            return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = homework.Id });
        }

        public async Task<IActionResult> ShowMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetViewModelForShowMyHomework(stageId, myId);

            return View(stage);
        }


        public async Task<IActionResult> ConfirmInterviewAppointments(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _myApplicationService.GetViewModelForConfirmInterviewAppointments(stageId, myId);

            return View(vm);
        }

        public async Task<IActionResult> ConfirmAppointmentsInInterview(string interviewAppointmentId, string returnUrl = null)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.ConfirmAppointmentsInInterview(interviewAppointmentId, myId);

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
        #endregion
    }
}