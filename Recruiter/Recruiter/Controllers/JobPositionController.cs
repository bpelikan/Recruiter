﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Recruiter.AttributeFilters;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Repositories;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
    public class JobPositionController : Controller
    {
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IMapper _mapper;
        private readonly IJobPositionService _jobPositionService;
        private readonly IStringLocalizer<JobPositionController> _stringLocalizer;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobPositionController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context, 
            IJobPositionRepository jobPositionRepository, 
            IMapper mapper,
            IJobPositionService jobPositionService,
            IStringLocalizer<JobPositionController> stringLocalizer)
        {
            _userManager = userManager;
            _context = context;
            _jobPositionRepository = jobPositionRepository;
            _mapper = mapper;
            _jobPositionService = jobPositionService;
            _stringLocalizer = stringLocalizer;
        }

        [Route("/[controller]")]
        [Route("{jobPositionActivity?}")]
        public IActionResult Index(string jobPositionActivity = "")
        {
            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = _jobPositionService.GetViewModelForIndexByJobPositionActivity(jobPositionActivity, userId);

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Route("{jobPositionId?}")]
        public async Task<IActionResult> Details(string jobPositionId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = await _jobPositionService.GetViewModelForJobPositionDetails(jobPositionId, userId);

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocal(returnUrl);
        }

        public async Task<IActionResult> Add(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
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
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddJobPositionViewModel addJobPositionViewModel, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (addJobPositionViewModel.StartDate.ToUniversalTime() < DateTime.UtcNow)
                    ModelState.AddModelError("StartDate", _stringLocalizer["StartDate must be in the future."]);
                if(addJobPositionViewModel.ApplicationStagesRequirement.IsApplicationApprovalRequired == false &&
                    addJobPositionViewModel.ApplicationStagesRequirement.IsHomeworkRequired == false &&
                    addJobPositionViewModel.ApplicationStagesRequirement.IsInterviewRequired == false &&
                    addJobPositionViewModel.ApplicationStagesRequirement.IsPhoneCallRequired == false)
                        ModelState.AddModelError("", _stringLocalizer["JobPosition must have at least one stage."]);
            }
                
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                try
                {
                    var jobPosition = await _jobPositionService.AddJobPosition(addJobPositionViewModel, userId);
                    TempData["Success"] = _stringLocalizer["Successfully created."].ToString();
                    return RedirectToAction(nameof(JobPositionController.Details), new { jobPositionId = jobPosition.Id, returnUrl });
                }
                catch (CustomRecruiterException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            var users = await _userManager.GetUsersInRoleAsync(RoleCollection.Recruiter);
            ViewData["DefaultResponsibleForApplicatioApprovalId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForApplicatioApprovalId);
            ViewData["DefaultResponsibleForPhoneCallId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForPhoneCallId);
            ViewData["DefaultResponsibleForHomeworkId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForHomeworkId);
            ViewData["DefaultResponsibleForInterviewId"] = new SelectList(users, "Id", "Email", addJobPositionViewModel.ApplicationStagesRequirement.DefaultResponsibleForInterviewId);

            return View(addJobPositionViewModel);
        }

        [Route("{jobPositionId?}")]
        public async Task<IActionResult> Edit(string jobPositionId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = await _jobPositionService.GetViewModelForEditJobPosition(jobPositionId, userId);

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [Route("{jobPositionId?}")]
        public async Task<IActionResult> Edit(string jobPositionId, EditJobPositionViewModel editJobPositionViewModel, string returnUrl = null)
        {
            if (ModelState.IsValid)
                if (editJobPositionViewModel.StartDate.ToUniversalTime() < DateTime.UtcNow)
                    ModelState.AddModelError("StartDate", _stringLocalizer["StartDate must be in the future."]);

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                try
                {
                    var jobPosition = await _jobPositionService.UpdateJobPosition(editJobPositionViewModel, userId);
                    TempData["Success"] = _stringLocalizer["Successfully updated."].ToString();

                    return RedirectToLocal(returnUrl);
                }
                catch (CustomRecruiterException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return View(editJobPositionViewModel);
        }

        [HttpPost]
        [Route("{jobPositionId?}")]
        public async Task<IActionResult> Delete(string jobPositionId, string returnUrl = null, string returnUrlFail = null)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _jobPositionService.RemoveJobPosition(jobPositionId, userId);
                TempData["Success"] = _stringLocalizer["Successfully deleted."].ToString();
                return RedirectToLocal(returnUrl);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if(returnUrlFail != null)
                return RedirectToLocal(returnUrlFail);
            return RedirectToLocal(returnUrl);
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
                return RedirectToAction(nameof(JobPositionController.Index));
            }
        }
        #endregion
    }
}