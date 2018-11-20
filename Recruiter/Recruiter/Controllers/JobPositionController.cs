using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Repositories;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
    public class JobPositionController : Controller
    {
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IMapper _mapper;
        private readonly IJobPositionService _jobPositionService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobPositionController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context, 
            IJobPositionRepository jobPositionRepository, 
            IMapper mapper,
            IJobPositionService jobPositionService)
        {
            _userManager = userManager;
            _context = context;
            _jobPositionRepository = jobPositionRepository;
            _mapper = mapper;
            _jobPositionService = jobPositionService;
        }

        public IActionResult Index(string jobPositionActivity = "")
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = _jobPositionService.GetViewModelForIndexByJobPositionActivity(jobPositionActivity, userId);

            return View(vm);

            //IEnumerable<JobPosition> jobPositions = null;
            //switch (jobPositionActivity)
            //{
            //    case JobPositionActivity.Active:
            //        jobPositions = _context.JobPositions.Where(x => x.StartDate <= DateTime.UtcNow && 
            //                                                        (x.EndDate >= DateTime.UtcNow || x.EndDate == null))
            //                                            .OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
            //        break;

            //    case JobPositionActivity.Planned:
            //        jobPositions = _context.JobPositions.Where(x => x.StartDate > DateTime.UtcNow)
            //                                            .OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
            //        break;

            //    case JobPositionActivity.Expired:
            //        jobPositions = _context.JobPositions.Where(x => x.EndDate < DateTime.UtcNow)
            //                                            .OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
            //        break;

            //    default:
            //        jobPositions = _context.JobPositions.OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
            //        break;
            //}

            //var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);

            //foreach (var jobPosition in vm)
            //{
            //    jobPosition.StartDate = jobPosition.StartDate.ToLocalTime();
            //    jobPosition.EndDate = jobPosition.EndDate?.ToLocalTime();
            //}

            //return View(vm);
        }

        public async Task<IActionResult> Details(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = await _jobPositionService.GetViewModelForJobPositionDetails(id, userId);

            return View(vm);

            //var jobPosition = await _jobPositionRepository.GetAsync(id);

            //if (jobPosition == null)
            //    return RedirectToAction(nameof(JobPositionController.Index));

            //var vm = _mapper.Map<JobPosition, JobPositionViewModel>(jobPosition);
            //vm.StartDate = vm.StartDate.ToLocalTime();
            //vm.EndDate = vm.EndDate?.ToLocalTime();

            //var applications = await _context.Applications.Include(x => x.User).Where(x => x.JobPositionId == jobPosition.Id).ToListAsync();
            //foreach (var application in applications)
            //{
            //    application.CreatedAt = application.CreatedAt.ToLocalTime();
            //    vm.AddApplication(application);
            //}

            //return View(vm);
        }

        public async Task<IActionResult> Add()
        {
            var vm = new AddJobPositionViewModel()
            {
                StartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 
                                            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 00).ToLocalTime()
            };

            var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
            ViewData["DefaultResponsibleForApplicatioApprovalId"] = new SelectList(users, "Id", "Email");
            ViewData["DefaultResponsibleForPhoneCallId"] = new SelectList(users, "Id", "Email");
            ViewData["DefaultResponsibleForHomeworkId"] = new SelectList(users, "Id", "Email");
            ViewData["DefaultResponsibleForInterviewId"] = new SelectList(users, "Id", "Email");

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddJobPositionViewModel addJobPositionViewModel)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(HttpContext.User);

                addJobPositionViewModel.ApplicationStagesRequirement.RemoveDefaultResponsibleIfStageIsDisabled();

                var jobPosition = new JobPosition()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = addJobPositionViewModel.Name,
                    Description = addJobPositionViewModel.Description,
                    StartDate = addJobPositionViewModel.StartDate.ToUniversalTime(),
                    EndDate = addJobPositionViewModel.EndDate?.ToUniversalTime(),
                    CreatorId = userId,
                    ApplicationStagesRequirement = addJobPositionViewModel.ApplicationStagesRequirement
                };

                await _jobPositionRepository.AddAsync(jobPosition);
                var jobPositionId = jobPosition.Id;

                if (jobPositionId != null)
                    return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPositionId });
                else
                    ModelState.AddModelError("", "Something went wrong while adding this job position.");
            }

            var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
            ViewData["DefaultResponsibleForApplicatioApprovalId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForApplicatioApprovalId);
            ViewData["DefaultResponsibleForPhoneCallId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForPhoneCallId);
            ViewData["DefaultResponsibleForHomeworkId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForHomeworkId);
            ViewData["DefaultResponsibleForInterviewId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForInterviewId);

            return View(addJobPositionViewModel);

            //throw new Exception()
            //return View(addJobPositionViewModel);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var jobPosition = await _jobPositionRepository.GetAsync(id);

            if (jobPosition == null)
            {
                throw new Exception($"Job position with id {id} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
                //ModelState.AddModelError("", "Something went wrong while getting job position for editing.");
                //return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
            }

            var vm = _mapper.Map<JobPosition, EditJobPositionViewModel>(jobPosition);
            vm.StartDate = vm.StartDate.ToLocalTime();
            vm.EndDate = vm.EndDate?.ToLocalTime();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditJobPositionViewModel editJobPositionViewModel)
        {
            if (!ModelState.IsValid)
                return View(editJobPositionViewModel);

            var jobPosition = await _jobPositionRepository.GetAsync(editJobPositionViewModel.Id);

            if (jobPosition != null)
            {
                jobPosition.Name = editJobPositionViewModel.Name;
                jobPosition.Description = editJobPositionViewModel.Description;
                jobPosition.StartDate = editJobPositionViewModel.StartDate.ToUniversalTime();
                jobPosition.EndDate = editJobPositionViewModel.EndDate?.ToUniversalTime();

                await _jobPositionRepository.UpdateAsync(jobPosition);

                return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPosition.Id });
            }

            throw new Exception($"Job position with id {editJobPositionViewModel.Id} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            //ModelState.AddModelError("", "Something went wrong while editing job position.");
            //return View(editJobPositionViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            //var jobPosition = await _jobPositionRepository.GetAsync(id);
            var jobPosition = await _context.JobPositions.Include(x => x.Applications).SingleOrDefaultAsync(x => x.Id == id);

            if (jobPosition.Applications.Count != 0)
            {
                throw new Exception($"Job position with id {id} has Applications. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            }

            if (jobPosition != null)
            {
                await _jobPositionRepository.RemoveAsync(jobPosition);
                return RedirectToAction(nameof(JobPositionController.Index));
            }

            throw new Exception($"Job position with id {id} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            //ModelState.AddModelError("", "Something went wrong while deleting this user.");
            //return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFromIndex(string id)
        {
            //var jobPosition = await _jobPositionRepository.GetAsync(id);
            var jobPosition = await _context.JobPositions.Include(x => x.Applications).SingleOrDefaultAsync(x => x.Id == id);

            if (jobPosition.Applications.Count != 0)
            {
                ModelState.AddModelError("", "This JobPosition has already Applications.");

                var jobPositions = await _jobPositionRepository.GetAllAsync();
                jobPositions = jobPositions.OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
                var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);

                return View(nameof(JobPositionController.Index), vm);
            }

            if (jobPosition != null)
            {
                await _jobPositionRepository.RemoveAsync(jobPosition);
                return RedirectToAction(nameof(JobPositionController.Index));
            }

            throw new Exception($"Job position with id {id} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            //ModelState.AddModelError("", "Something went wrong while deleting this user.");
            //return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
        }
    }
}