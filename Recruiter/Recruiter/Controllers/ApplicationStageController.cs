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
            if (stageName == "Homework")
                return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReviewHomework), new { stageName = "Homework"});

            var myId = _userManager.GetUserId(HttpContext.User);

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                stagesSortedByName.Add(new StagesViewModel()
                {
                    Name = t.Name,
                    Quantity = _context.ApplicationStages
                                            .AsNoTracking()
                                            .Where(x => x.State == ApplicationStageState.InProgress &&
                                                        x.ResponsibleUserId == myId &&
                                                        x.GetType().Name == t.Name).Count(),
                });
            }

            var stages = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .Where(x => x.State == ApplicationStageState.InProgress &&
                                                    x.ResponsibleUserId == myId &&
                                                    (x.GetType().Name == stageName || stageName == ""));

            var vm = new ApplicationsStagesToReviewViewModel()
            {
                StageSortedByName = stagesSortedByName,
            };
            vm.AsignedStages = new List<AsignedStagesViewModel>();
            foreach (var stage in stages)
            {
                vm.AsignedStages.Add(new AsignedStagesViewModel()
                {
                    Application = new ApplicationViewModel() {
                        Id = stage.Application.Id,
                        CreatedAt = stage.Application.CreatedAt,
                        User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                        JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                    },
                    CurrentStage = stage,
                });
            }
            vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();
            
            return View(vm);
        }

        public IActionResult ApplicationsStagesToReviewHomework(string stageName = "Homework")
        {
            var myId = _userManager.GetUserId(HttpContext.User);

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                stagesSortedByName.Add(new StagesViewModel()
                {
                    Name = t.Name,
                    Quantity = _context.ApplicationStages
                                            .AsNoTracking()
                                            .Where(x => x.State == ApplicationStageState.InProgress &&
                                                        x.ResponsibleUserId == myId &&
                                                        x.GetType().Name == t.Name).Count(),
                });
            }

            var stages = _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .Where(x => x.State == ApplicationStageState.InProgress &&
                                                    x.ResponsibleUserId == myId &&
                                                    (x.GetType().Name == stageName || stageName == ""));

            var vm = new ApplicationsStagesToReviewViewModel()
            {
                StageSortedByName = stagesSortedByName,
            };
            vm.AsignedStages = new List<AsignedStagesViewModel>();
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
                        CreatedAt = stage.Application.CreatedAt,
                        User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                        JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                    },
                    CurrentStage = stage,
                });
            }

            vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();
            return View(vm);
        }
        #endregion

        #region AssingUserToApplicationStage()
        public async Task<IActionResult> AssingUserToApplicationStage(string stageId)
        {
            var stage = await _context.ApplicationStages.Include(x => x.Application).FirstOrDefaultAsync(x => x.Id == stageId);

            if (stage != null)
            {
                var vm = new AssingUserToStageViewModel()
                {
                    ApplicationId = stage.ApplicationId,
                    StageId = stage.Id,
                };

                var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
                if(users.Count() != 0)
                    ViewData["UsersToAssingToStage"] = new SelectList(users, "Id", "Email");
                return View(vm);
            }

            throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
        }

        [HttpPost]
        public async Task<IActionResult> AssingUserToApplicationStage(AssingUserToStageViewModel addResponsibleUserToStageViewModel)
        {
            if (!ModelState.IsValid)
                return View(addResponsibleUserToStageViewModel);

            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == addResponsibleUserToStageViewModel.StageId);

            if (stage.State == ApplicationStageState.InProgress)
            {
                throw new Exception($"Can't change ResponsibleUser in ApplicationStage with ID: {addResponsibleUserToStageViewModel.StageId} that is InProgress state. (UserID: {_userManager.GetUserId(HttpContext.User)})");

                //ModelState.AddModelError("", "You can't change responsible user in application state that is in progress.");
                //return View(addResponsibleUserToStageViewModel);
            }

            if (stage != null)
            {
                var firstStageInThisApplicationId = _context.ApplicationStages.Where(x => x.ApplicationId == stage.ApplicationId).OrderBy(x => x.Level).First().Id;

                stage.ResponsibleUserId = addResponsibleUserToStageViewModel.UserId;
                if (stage.Id == firstStageInThisApplicationId)
                    stage.State = ApplicationStageState.InProgress;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ApplicationController.ApplicationDetails), "Application", new { id = addResponsibleUserToStageViewModel.ApplicationId });
            }

            throw new Exception($"ApplicationStage with id {addResponsibleUserToStageViewModel.StageId} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
        }
        #endregion

        #region ProcessStage()
        public async Task<IActionResult> ProcessStage(string stageId)
        {
            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == stageId);

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
            var stage = await _applicationStageService.GetApplicationStageBaseToShowInProcessStage(stageId, myId) as Homework;

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

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
        }
        #endregion

        #region ApplicationApproval
        public async Task<IActionResult> ProcessInterview(string stageId)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            var vm = await _applicationStageService.GetViewModelForProcessInterview(stageId, myId);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessInterview(ProcessInterviewViewModel interviewViewModel, bool accepted = false)
        {
            var myId = _userManager.GetUserId(HttpContext.User);
            await _applicationStageService.UpdateInterview(interviewViewModel, accepted, myId);

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
                    return RedirectToAction(nameof(ApplicationStageController.ApplicationStageBaseDatails), 
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
            var stage = await _applicationStageService.GetApplicationStageBaseWithInclude(stageId, myId);
            
            return View(stage);
        }

        public async Task<IActionResult> HomeworkStageDetails(string stageId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var myId = _userManager.GetUserId(HttpContext.User);
            var stage = await _applicationStageService.GetApplicationStageBaseWithInclude(stageId, myId);
            
            return View(stage);
        }

        #endregion

    }
}