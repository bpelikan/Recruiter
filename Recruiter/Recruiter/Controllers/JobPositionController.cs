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

        }

        public async Task<IActionResult> Details(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = await _jobPositionService.GetViewModelForJobPositionDetails(id, userId);

            return View(vm);

        }

        public async Task<IActionResult> Add()
        {

            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = _jobPositionService.GetViewModelForAddJobPosition(userId);

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
            if (!ModelState.IsValid)
            {
                var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
                ViewData["DefaultResponsibleForApplicatioApprovalId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForApplicatioApprovalId);
                ViewData["DefaultResponsibleForPhoneCallId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForPhoneCallId);
                ViewData["DefaultResponsibleForHomeworkId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForHomeworkId);
                ViewData["DefaultResponsibleForInterviewId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForInterviewId);

                return View(addJobPositionViewModel);
            }

            var userId = _userManager.GetUserId(HttpContext.User);
            var jobPosition = await _jobPositionService.AddJobPosition(addJobPositionViewModel, userId);

            return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPosition.Id });

        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            var vm = await _jobPositionService.GetViewModelForEditJobPosition(id, userId);

            return View(vm);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditJobPositionViewModel editJobPositionViewModel)
        {
            if (!ModelState.IsValid)
                return View(editJobPositionViewModel);

            var userId = _userManager.GetUserId(HttpContext.User);
            var jobPosition = await _jobPositionService.UpdateJobPosition(editJobPositionViewModel, userId);

            return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPosition.Id });

        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            await _jobPositionService.RemoveJobPosition(id, userId);

            return RedirectToAction(nameof(JobPositionController.Index));

        }

        [HttpPost]
        public async Task<IActionResult> DeleteFromIndex(string id, string jobPositionActivity = "") //-> DeleteFromIndexView
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            //IEnumerable<JobPositionViewModel> vm;

            try
            {
                await _jobPositionService.RemoveJobPositionFromIndexView(id, userId);
            }
            catch (ApplicationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                //var jobPositions = await _jobPositionRepository.GetAllAsync();
                //jobPositions = jobPositions.OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
                //var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);
                //return View(nameof(JobPositionController.Index), vm);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong, try again.");
            }

            var vm = _jobPositionService.GetViewModelForIndexByJobPositionActivity(jobPositionActivity, userId);
            return View(nameof(JobPositionController.Index), vm);


            //catch (Exception ex)
            //{

            //}


            //return RedirectToAction(nameof(JobPositionController.Index));

        }
    }
}