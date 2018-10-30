using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    public class ApplicationStageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationStageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
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
        [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        public async Task<IActionResult> AssingUserToApplicationStage(AssingUserToStageViewModel addResponsibleUserToStageViewModel)
        {
            if (!ModelState.IsValid)
                return View(addResponsibleUserToStageViewModel);

            var stage = await _context.ApplicationStages.FirstOrDefaultAsync(x => x.Id == addResponsibleUserToStageViewModel.StageId);

            if (stage != null)
            {
                stage.ResponsibleUserId = addResponsibleUserToStageViewModel.UserId;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ApplicationController.ApplicationDetails), "Application", new { id = addResponsibleUserToStageViewModel.ApplicationId });
            }

            throw new Exception($"ApplicationStage with id {addResponsibleUserToStageViewModel.StageId} not found. (UserID: {_userManager.GetUserId(HttpContext.User)})");
        }
    }
}