﻿using System;
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

        public ApplicationStageController(IMapper mapper, ICvStorageService cvStorageService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _cvStorageService = cvStorageService;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //[Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        public IActionResult ApplicationsStagesToReview(string stageName = "")
        {
            
            var myId = _userManager.GetUserId(HttpContext.User);

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                stagesSortedByName.Add(new StagesViewModel()
                {
                    Name = t.Name,
                    Quantity = _context.Applications
                                            .Include(x => x.JobPosition)
                                            .Include(x => x.User)
                                            .Include(x => x.ApplicationStages)
                                            .Where(x =>
                                                        x.ApplicationStages
                                                            .OrderBy(y => y.Level)
                                                            .Skip(x.ApplicationStages.Where(y => y.State == ApplicationStageState.Finished).Count())
                                                            .Take(1)
                                                            .Any(y => x.Id == y.ApplicationId &&
                                                                        (y.GetType().Name == t.Name) &&
                                                                        y.State != ApplicationStageState.Finished &&
                                                                        y.ResponsibleUserId == myId)

                                            )
                                            .Count(),
                });
            }

            var applications = _context.Applications
                                            .Include(x => x.JobPosition)
                                            .Include(x => x.User)
                                            .Include(x => x.ApplicationStages)
                                            .Where(x =>
                                                        x.ApplicationStages
                                                            .OrderBy(y => y.Level)
                                                            .Skip(x.ApplicationStages.Where(y => y.State == ApplicationStageState.Finished).Count())
                                                            .Take(1)
                                                            .Any(y => x.Id == y.ApplicationId &&
                                                                        (y.GetType().Name == stageName || stageName == "") &&
                                                                        y.State != ApplicationStageState.Finished &&
                                                                        y.ResponsibleUserId == myId)
                                            );

            var vm = new ApplicationsStagesToReviewViewModel()
            {
                Stages = stagesSortedByName,
            };
            vm.Applications = new List<ApplicationViewModel>();
            foreach (var app in applications)
            {
                vm.Applications.Add(new ApplicationViewModel()
                {
                    Id = app.Id,
                    CreatedAt = app.CreatedAt.ToLocalTime(),
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(app.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(app.JobPosition),
                });
            }

            return View(vm);
        }


        //[Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
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
        //[Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
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
                ApplicationStages = stage.Application.ApplicationStages.Where(x => x.State == ApplicationStageState.Finished).OrderBy(x => x.Level).ToArray(),
                StageToProcess = _mapper.Map<ApplicationStageBase, ApplicationApprovalViewModel>(stage)
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

            var application = await _context.Applications
                                                .Include(x => x.ApplicationStages)
                                                .FirstOrDefaultAsync(x => x.Id == stage.ApplicationId);
            if(application == null)
                throw new Exception($"Application with id {stage.Application.Id} not found. (UserID: {myId})");

            if (application.ApplicationStages.Count() != 0)
            {
                var nextStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State != ApplicationStageState.Finished).First();
                if (nextStage.State != ApplicationStageState.Finished)
                {
                    nextStage.State = ApplicationStageState.InProgress;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(ApplicationStageController.ApplicationsStagesToReview), new { stageName = "ApplicationApproval" });
        }
    }
}