using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Recruiter.CustomExceptions;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationViewModels;
using Recruiter.Models.ApplicationViewModels.Shared;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
    [Route("[controller]/[action]")]
    public class ApplicationController : Controller
    {
        private readonly ICvStorageService _cvStorageService;
        private readonly IMapper _mapper;
        private readonly IApplicationService _applicationService;
        private readonly IStringLocalizer<ApplicationController> _stringLocalizer;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ApplicationController(
                    ICvStorageService cvStorageService, 
                    IMapper mapper, 
                    IApplicationService applicationService,
                    IStringLocalizer<ApplicationController> stringLocalizer,
                    UserManager<ApplicationUser> userManager, 
                    ApplicationDbContext context)
        {
            _cvStorageService = cvStorageService;
            _mapper = mapper;
            _applicationService = applicationService;
            _stringLocalizer = stringLocalizer;
            _userManager = userManager;
            _context = context;
        }

        [Route("/[controller]")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ApplicationController.Applications));
        }

        [Route("{stageName?}")]
        public IActionResult Applications(string stageName = "")
        {
            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = _applicationService.GetViewModelForApplications(stageName, userId);

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Route("{applicationId?}")]
        public async Task<IActionResult> ApplicationDetails(string applicationId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                var vm = await _applicationService.GetViewModelForApplicationDetails(applicationId, userId);

                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [Route("{applicationId?}")]
        public async Task<IActionResult> DeleteApplication(string applicationId, string returnUrl = null, string returnUrlFail = null)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                await _applicationService.DeleteApplication(applicationId, userId);
                TempData["Success"] = _stringLocalizer["Successfully deleted."].ToString();
                return RedirectToLocal(returnUrl);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (returnUrlFail != null)
                return RedirectToLocal(returnUrlFail);
            return RedirectToLocal(returnUrl);
        }

        [Route("{applicationId?}")]
        public async Task<IActionResult> ApplicationsViewHistory(string applicationId, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var userId = _userManager.GetUserId(HttpContext.User);
            try
            {
                var vm = await _applicationService.GetViewModelForApplicationsViewHistory(applicationId, userId);
                return View(vm);
            }
            catch (CustomRecruiterException ex)
            {
                TempData["Error"] = ex.Message;
            }

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
                return RedirectToAction(nameof(ApplicationController.Index));
            }
        }
        #endregion

    }
}