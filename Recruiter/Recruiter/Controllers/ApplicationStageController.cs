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

        public IActionResult ApplicationsStagesToReview(string stageName = "")
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
                    CurrentStage = _mapper.Map<ApplicationStageBase, ApplicationStageViewModel>(stage),
                });
            }
            vm.AsignedStages = vm.AsignedStages.OrderBy(x => x.Application.CreatedAt).ToList();
            return View(vm);
        }

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
                stage.ResponsibleUserId = addResponsibleUserToStageViewModel.UserId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ApplicationController.ApplicationDetails), "Application", new { id = addResponsibleUserToStageViewModel.ApplicationId });
            }

            throw new Exception($"ApplicationStage with id {addResponsibleUserToStageViewModel.StageId} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
        }

        public async Task<IActionResult> ProcessStage(string stageId)
        {
            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == stageId);

            switch (stage.GetType().Name) {
                case "ApplicationApproval":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessApplicationApproval), new { stageId = stage.Id });
                case "PhoneCall":
                    return RedirectToAction(nameof(ApplicationStageController.ProcessPhoneCall), new { stageId = stage.Id });
                case "Homework":

                case "Interview":

                default:

                    return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview));
            }
        }

        public async Task<IActionResult> ProcessApplicationApproval(string stageId)
        {
            var stage = await _context.ApplicationStages
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.ApplicationStages)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId);

            var vm = new ProcessApplicationApprovalViewModel()
            {
                Application = new ApplicationViewModel() {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = stage.Application.ApplicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, ApplicationApprovalViewModel>(stage),
                ApplicationStagesWaiting = stage.Application.ApplicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessApplicationApproval(ProcessApplicationApprovalViewModel applicationApprovalViewModel)
        {
            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == applicationApprovalViewModel.StageToProcess.Id);
            var myId = _userManager.GetUserId(HttpContext.User);

            if (stage == null)
                throw new Exception($"ApplicationStage with id {applicationApprovalViewModel.StageToProcess.Id} not found. (UserID: {myId})");
            if(stage.ResponsibleUserId != myId)
                throw new Exception($"User with ID: {myId} is not allowed to process ApplicationStage with ID: {applicationApprovalViewModel.StageToProcess.Id} not found. (UserID: {myId})");

            stage.Note = applicationApprovalViewModel.StageToProcess.Note;
            stage.Rate = applicationApprovalViewModel.StageToProcess.Rate;
            stage.Accepted = applicationApprovalViewModel.StageToProcess.Accepted;
            stage.AcceptedById = myId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await _applicationStageService.UpdateNextApplicationStageState(stage.ApplicationId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "ApplicationApproval" });
        }

        public async Task<IActionResult> ProcessPhoneCall(string stageId)
        {
            var stage = await _context.ApplicationStages
                                    //.Include(x => x.Application)
                                    //    .ThenInclude(x => x.ApplicationStages)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId);

            var applicationStages = _context.ApplicationStages
                                                .Include(x => x.AcceptedBy)
                                                .Include(x => x.ResponsibleUser)
                                                .Where(x => x.ApplicationId == stage.ApplicationId);

            var vm = new ProcessPhoneCallViewModel()
            {
                Application = new ApplicationViewModel()
                {
                    Id = stage.Application.Id,
                    CreatedAt = stage.Application.CreatedAt,
                    CvFileName = stage.Application.CvFileName,
                    CvFileUrl = _cvStorageService.UriFor(stage.Application.CvFileName),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(stage.Application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(stage.Application.JobPosition),
                },
                ApplicationStagesFinished = applicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, PhoneCallViewModel>(stage),
                ApplicationStagesWaiting = applicationStages.Where(x => x.State == ApplicationStageState.Waiting).OrderBy(x => x.Level).ToArray()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPhoneCall(ProcessPhoneCallViewModel phoneCallViewModel)
        {
            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == phoneCallViewModel.StageToProcess.Id);
            var myId = _userManager.GetUserId(HttpContext.User);

            if (stage == null)
                throw new Exception($"ApplicationStage with id {phoneCallViewModel.StageToProcess.Id} not found. (UserID: {myId})");
            if (stage.ResponsibleUserId != myId)
                throw new Exception($"User with ID: {myId} is not allowed to process ApplicationStage with ID: {phoneCallViewModel.StageToProcess.Id} not found. (UserID: {myId})");

            stage.Note = phoneCallViewModel.StageToProcess.Note;
            stage.Rate = phoneCallViewModel.StageToProcess.Rate;
            stage.Accepted = phoneCallViewModel.StageToProcess.Accepted;
            stage.AcceptedById = myId;
            stage.State = ApplicationStageState.Finished;
            await _context.SaveChangesAsync();

            await _applicationStageService.UpdateNextApplicationStageState(stage.ApplicationId);

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "PhoneCall" });
        }
    }
}