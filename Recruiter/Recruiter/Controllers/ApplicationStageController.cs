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
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
    public class ApplicationStageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ApplicationStageController(IMapper mapper, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
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
            //List<string> test = new List<string>();
            //foreach (Type t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            //{
            //    if (t.IsSubclassOf(typeof(ApplicationStageBase)))
            //        test.Add(t.Name);
            //}
            //IEnumerable<string> test = Assembly.GetExecutingAssembly()
            //                        .GetTypes()
            //                        .Where(x => x.IsSubclassOf(typeof(ApplicationStageBase)));
            var myId = _userManager.GetUserId(HttpContext.User);

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                //var test = _context.Applications
                //                            .Include(x => x.JobPosition)
                //                            .Include(x => x.User)
                //                            .Include(x => x.ApplicationStages)
                //                            .Where(x =>
                //                                        x.ApplicationStages
                //                                            .OrderBy(y => y.Level)
                //                                            .Skip(x.ApplicationStages.Where(y => y.State == ApplicationStageState.Finished).Count())
                //                                            .Take(1)
                //                                            .Any(y => x.Id == y.ApplicationId &&
                //                                                        (y.GetType().Name == t.Name) &&
                //                                                        y.State != ApplicationStageState.Finished &&
                //                                                        y.ResponsibleUserId == myId)

                //                            )
                //                            .ToList();

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
                    //Quantity = _context.ApplicationStages
                    //                .Where(x => x.GetType().Name == t.Name &&
                    //                            x.State != ApplicationStageState.Finished &&
                    //                            x.ResponsibleUserId == myId)
                    //                .Count(),
                });
            }

            //List<Type> derivedTypes = ApplicationStageBase.GetDerivedTypes(typeof(BaseClass<>);

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
            //.ToList();

            //var applications = _context.Applications
            //                                .Include(x => x.JobPosition)
            //                                .Include(x => x.User)
            //                                .Include(x => x.ApplicationStages)
            //                                .Where(x => x.ApplicationStages
            //                                                .OrderBy(y => y.Level)
            //                                                .Any(y => (y.GetType().Name == stageName || stageName == "") &&
            //                                                            y.State != ApplicationStageState.Finished &&
            //                                                            y.ResponsibleUserId == myId)
            //                                );

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


            //var vm = new ApplicationsViewModel()
            //{
            //    Applications = _mapper.Map<IEnumerable<Application>, IEnumerable<ApplicationViewModel>>(applications),
            //    Stages = stages,
            //};

            //foreach (var application in vm.Applications)
            //    application.CreatedAt = application.CreatedAt.ToLocalTime();

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
    }
}