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
            var vm = _myApplicationService.GetViewModelForMyApplications(userId);
            //var applications = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).Where(x => x.UserId == userId);

            //var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<MyApplicationsViewModel>>(applications);
            //foreach (var application in vm)
            //    application.CreatedAt = application.CreatedAt.ToLocalTime();

            return View(vm);
        }

        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<ActionResult> MyApplicationDetails(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var application = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).Include(x => x.ApplicationStages).FirstOrDefault(x => x.Id == id);
            var userId = _userManager.GetUserId(HttpContext.User);

            if (application != null && userId == application.UserId)
            {
                await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    ViewTime = DateTime.UtcNow,
                    ApplicationId = application.Id,
                    UserId = _userManager.GetUserId(HttpContext.User)
                });
                await _context.SaveChangesAsync();

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
                                                    .OrderBy(x => x.Level).ToList()
                };

                return View(vm);
            }

            //Add error: It's not yours application/cv 
            //redirect to my application list
            return RedirectToAction(nameof(OfferController.Index));
        }

        [HttpPost]
        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<IActionResult> DeleteMyApplication(string id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                throw new Exception($"Application with id: {id} doesn't exist.");
            }
            if (application.UserId != userId)
            {
                throw new Exception($"User with id: {userId} aren't owner of application with id: {application.Id}.");
            }

            var delete = await _cvStorageService.DeleteCvAsync(application.CvFileName);
            if (!delete)
            {
                throw new Exception($"Something went wrong while deleting cv in Blob: {application.CvFileName}.");
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyApplicationController.MyApplications));
        }

        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<IActionResult> Apply(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var offer = await _context.JobPositions.SingleOrDefaultAsync(x => x.Id == id);
            if (offer == null)
                return RedirectToAction(nameof(OfferController.Index));

            var vm = new ApplyApplicationViewModel()
            {
                JobPositionId = offer.Id,
                JobPositionName = offer.Name,
            };
            return View(vm);
        }

        [HttpPost]
        //[Authorize(Roles = RoleCollection.Recruit)]
        public async Task<IActionResult> Apply(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel)
        {
            if (!ModelState.IsValid)
                return View(applyApplicationViewModel);

            if (cv != null)
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                using (var stream = cv.OpenReadStream())
                {
                    var CvFileName = await _cvStorageService.SaveCvAsync(stream, userId, cv.FileName);
                    applyApplicationViewModel.CvFileName = CvFileName;
                }

                if (Path.GetExtension(cv.FileName) != ".pdf")
                {
                    ModelState.AddModelError("", "CV must have .pdf extension.");
                    return View(applyApplicationViewModel);
                }
                if (applyApplicationViewModel.CvFileName == null)
                {
                    ModelState.AddModelError("", "Something went wrong during uploading CV, try again or contact with admin.");
                    return View(applyApplicationViewModel);
                }

                if (await _context.Applications
                                    .Where(x => x.UserId == userId && x.JobPositionId == applyApplicationViewModel.JobPositionId).CountAsync() != 0)
                {
                    ModelState.AddModelError("", "You have already sent application to this offer.");
                    return View(applyApplicationViewModel);
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

                await _applicationStageService.AddRequiredStagesToApplication(application.Id);

                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = application.Id });
            }

            ModelState.AddModelError("", "CV file not found.");
            return View(applyApplicationViewModel);
        }

        public async Task<IActionResult> ProcessMyHomework(string stageId)
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

            var vm = new Homework()
            {
                Id = stage.Id,
                Duration = stage.Duration,
                Description = "Description is hidden, clicking ,,Show description\" button will start time counting and show you the content of the homework",
                ApplicationId = stage.ApplicationId,
            };

            if(stage.HomeworkState == HomeworkState.WaitingForRead)
                return View(vm);
            else
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
        }

        [HttpPost]
        public async Task<IActionResult> BeforeReadMyHomework(Homework homework)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefaultAsync(x => x.Id == homework.Id) as Homework;
            if (stage == null)
                throw new Exception($"ApplicationStage with id {homework.Id} not found. (UserID: {myId})");
            if (stage.Application.User.Id != myId)
                throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {homework.Id}.");

            if (stage.HomeworkState == HomeworkState.WaitingForRead)
            {
                stage.StartTime = DateTime.UtcNow;
                stage.EndTime = stage.StartTime?.AddHours(stage.Duration);
                stage.HomeworkState = HomeworkState.WaitingForSendHomework;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyApplicationController.ReadMyHomework), new { stageId = stage.Id });
        }

        public async Task<IActionResult> ReadMyHomework(string stageId)
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

            stage.StartTime = stage.StartTime?.ToLocalTime();
            stage.EndTime = stage.EndTime?.ToLocalTime();

            if(stage.HomeworkState == HomeworkState.WaitingForSendHomework)
                return View(stage);
            else
                return RedirectToAction(nameof(MyApplicationController.MyApplicationDetails), new { id = stage.ApplicationId });
        }

        [HttpPost]
        public async Task<IActionResult> SendMyHomework(Homework homework)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .FirstOrDefaultAsync(x => x.Id == homework.Id) as Homework;
            if (stage == null)
                throw new Exception($"ApplicationStage with id {homework.Id} not found. (UserID: {myId})");
            if (stage.Application.User.Id != myId)
                throw new Exception($"User with ID: {myId} is not allowed to get ApplicationStage with ID: {homework.Id}.");

            if (stage.HomeworkState == HomeworkState.WaitingForSendHomework)
            {
                stage.SendingTime = DateTime.UtcNow;
                stage.Url = homework.Url;
                stage.HomeworkState = HomeworkState.Completed;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyApplicationController.ShowMyHomework), new { stageId = stage.Id });
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