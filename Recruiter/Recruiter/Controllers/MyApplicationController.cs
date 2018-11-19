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
        private readonly IApplicationStageService _applicationStageService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MyApplicationController(
            IMyApplicationService myApplicationService,
            ICvStorageService cvStorageService, 
            IMapper mapper,
            IApplicationStageService applicationStageService,
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context)
        {
            _myApplicationService = myApplicationService;
            _cvStorageService = cvStorageService;
            _mapper = mapper;
            _applicationStageService = applicationStageService;
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
                ModelState.AddModelError("", "Something went wrong, try again.");
            }

            return View(applyApplicationViewModel);
        }

        public async Task<IActionResult> ProcessMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetHomeworkStageToShowInProcessMyHomework(stageId, myId);

            #region del
            //var stage = await _context.ApplicationStages
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.User)
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.JobPosition)
            //                        .AsNoTracking()
            //                        .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            //if (stage == null)
            //    throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {myId})");
            //if (stage.Application.User.Id != myId)
            //    throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {stageId}.");
            #endregion

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

            #region del
            //var stage = await _context.ApplicationStages
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.User)
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.JobPosition)
            //                        .AsNoTracking()
            //                        .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            //if (stage == null)
            //    throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {myId})");
            //if (stage.Application.User.Id != myId)
            //    throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {stageId}.");

            //var vm = new Homework()
            //{
            //    Id = stage.Id,
            //    Duration = stage.Duration,
            //    Description = "Description is hidden, clicking ,,Show description\" button will start time counting and show you the content of the homework",
            //    ApplicationId = stage.ApplicationId,
            //};

            //if(stage.HomeworkState == HomeworkState.WaitingForRead)
            //    return View(vm);
            //else
            //    return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
            #endregion
        }

        [HttpPost]
        public async Task<IActionResult> BeforeReadMyHomework(Homework homework)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.UpdateMyHomeworkAsReaded(homework.Id, myId);

            return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = homework.Id });

            #region del
            //var stage = await _context.ApplicationStages
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.User)
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.JobPosition)
            //                        .FirstOrDefaultAsync(x => x.Id == homework.Id) as Homework;
            //if (stage == null)
            //    throw new Exception($"ApplicationStage with id {homework.Id} not found. (UserID: {myId})");
            //if (stage.Application.User.Id != myId)
            //    throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {homework.Id}.");

            //if (stage.HomeworkState == HomeworkState.WaitingForRead)
            //{
            //    stage.StartTime = DateTime.UtcNow;
            //    stage.EndTime = stage.StartTime?.AddHours(stage.Duration);
            //    stage.HomeworkState = HomeworkState.WaitingForSendHomework;
            //    await _context.SaveChangesAsync();
            //}

            //return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = stage.Id });
            #endregion
        }

        public async Task<IActionResult> ReadMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _myApplicationService.GetViewModelForReadMyHomework(stageId, myId);

            return View(stage);

            #region del
            //var stage = await _context.ApplicationStages
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.User)
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.JobPosition)
            //                        .AsNoTracking()
            //                        .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            //if (stage == null)
            //    throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {myId})");
            //if (stage.Application.User.Id != myId)
            //    throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {stageId}.");

            //stage.StartTime = stage.StartTime?.ToLocalTime();
            //stage.EndTime = stage.EndTime?.ToLocalTime();

            //if(stage.HomeworkState == HomeworkState.WaitingForSendHomework)
            //    return View(stage);
            //else
            //    return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
            #endregion
        }

        [HttpPost]
        public async Task<IActionResult> SendMyHomework(Homework homework)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _myApplicationService.SendMyHomework(homework, myId);

            return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = homework.Id });

            #region del
            //var stage = await _context.ApplicationStages
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.User)
            //                        .Include(x => x.Application)
            //                            .ThenInclude(x => x.JobPosition)
            //                        .FirstOrDefaultAsync(x => x.Id == homework.Id) as Homework;
            //if (stage == null)
            //    throw new Exception($"ApplicationStage with id {homework.Id} not found. (UserID: {myId})");
            //if (stage.Application.User.Id != myId)
            //    throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {homework.Id}.");

            //if (stage.HomeworkState == HomeworkState.WaitingForSendHomework)
            //{
            //    stage.SendingTime = DateTime.UtcNow;
            //    stage.Url = homework.Url;
            //    stage.HomeworkState = HomeworkState.Completed;
            //    await _context.SaveChangesAsync();
            //}

            //return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = stage.Id });
            #endregion
        }

        public async Task<IActionResult> ShowMyHomework(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId) as Homework;
            if (stage == null)
                throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {myId})");
            if (stage.Application.User.Id != myId)
                throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {stageId}.");

            if (stage.HomeworkState != HomeworkState.Completed)
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });

            stage.StartTime = stage.StartTime?.ToLocalTime();
            stage.EndTime = stage.EndTime?.ToLocalTime();
            stage.SendingTime = stage.SendingTime?.ToLocalTime();

            if(stage.HomeworkState == HomeworkState.Completed)
                return View(stage);
            else
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
        }
    }
}


//switch (stage.HomeworkState)
//{
//    case HomeworkState.WaitingForRead:
//        return RedirectToAction(nameof(MyApplicationController.BeforeReadMyHomework), new { stageId = stage.Id });
//    case HomeworkState.WaitingForSendHomework:
//        return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = stage.Id });
//    case HomeworkState.Completed:
//        return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = stage.Id });
//    default:
//        return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
//}