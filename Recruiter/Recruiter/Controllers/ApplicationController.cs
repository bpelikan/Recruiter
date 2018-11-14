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
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.ApplicationViewModels;
using Recruiter.Models.ApplicationViewModels.Shared;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly ICvStorageService _cvStorageService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ApplicationController(ICvStorageService cvStorageService, IMapper mapper, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _cvStorageService = cvStorageService;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            //redirect
            //return View();
            return RedirectToAction(nameof(ApplicationController.Applications));
        }

        //[Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        //public IActionResult Applications()
        //{
        //    var applications = _context.Applications.Include(x => x.JobPosition).Include(x => x.User);
        //    var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<ApplicationsViewModel>>(applications);
        //    foreach (var application in vm)
        //        application.CreatedAt = application.CreatedAt.ToLocalTime();

        //    return View(vm);
        //}

        [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        public IActionResult Applications(string stageName = "")
        {
            var applications = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Include(x => x.ApplicationStages);

            List<StagesViewModel> stagesSortedByName = new List<StagesViewModel>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(ApplicationStageBase))))
            {
                stagesSortedByName.Add(new StagesViewModel()
                {
                    Name = t.Name,
                    Quantity = _context.ApplicationStages
                                            .AsNoTracking()
                                            .Where(x => x.State == ApplicationStageState.InProgress &&
                                                        x.GetType().Name == t.Name).Count(),
                });
            }

            var vm = new ApplicationsGroupedByStagesViewModel()
            {
                ApplicationStagesGroupedByName = stagesSortedByName,
            };

            vm.Applications = new List<ApplicationsViewModel>();
            foreach (var application in applications)
            {
                var currentStage = application.ApplicationStages.Where(x => x.State != ApplicationStageState.Finished).OrderBy(x => x.Level).FirstOrDefault();
                if (stageName == "" || stageName == currentStage.GetType().Name)
                {
                    vm.Applications.Add(new ApplicationsViewModel()
                    {
                        Id = application.Id,
                        CreatedAt = application.CreatedAt.ToLocalTime(),
                        JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                        User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                        CurrentStage = currentStage?.GetType().Name,
                        CurrentStageIsAssigned = currentStage?.ResponsibleUserId != null ? true : false
                    });
                }
            }

            return View(vm);

            //var applications = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).Include(x => x.ApplicationStages);
            //var vm = new List<ApplicationsViewModel>();

            //foreach (var application in applications)
            //{
            //    var currentStage = application.ApplicationStages.Where(x => x.State != ApplicationStageState.Finished).OrderBy(x => x.Level).FirstOrDefault();
            //    vm.Add(new ApplicationsViewModel()
            //    {
            //        Id = application.Id,
            //        CreatedAt = application.CreatedAt.ToLocalTime(),
            //        JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
            //        User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
            //        CurrentStage = currentStage?.GetType().Name,
            //        CurrentStageIsAssigned = currentStage?.ResponsibleUserId != null ? true : false
            //    });
            //}

            //var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<ApplicationsViewModel>>(applications);
            //foreach (var application in vm)
            //    application.CreatedAt = application.CreatedAt.ToLocalTime();
        }

        [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        public async Task<ActionResult> ApplicationDetails(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var application = _context.Applications
                                .Include(x => x.JobPosition)
                                .Include(x => x.User)
                                .FirstOrDefault(x => x.Id == id);

            if (application != null)
            {
                await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    ViewTime = DateTime.UtcNow,
                    ApplicationId = application.Id,
                    UserId = _userManager.GetUserId(HttpContext.User)
                });
                await _context.SaveChangesAsync();

                var applicationStages = _context.ApplicationStages
                                            .Include(x => x.ResponsibleUser)
                                            .Include(x => x.AcceptedBy)
                                            .Where(x => x.ApplicationId == application.Id).OrderBy(x => x.Level);

                var viewHistories = await _context.ApplicationsViewHistories
                                                    .Where(x => x.ApplicationId == application.Id)
                                                    .OrderByDescending(x => x.ViewTime)
                                                    .Take(10)
                                                    .ToListAsync();
                foreach (var viewHistory in viewHistories)
                    viewHistory.ViewTime = viewHistory.ViewTime.ToLocalTime();
                
                var vm = new ApplicationDetailsViewModel()
                {
                    Id = application.Id,
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                    CvFileUrl = _cvStorageService.UriFor(application.CvFileName),
                    CreatedAt = application.CreatedAt.ToLocalTime(),
                    ApplicationsViewHistories = viewHistories,
                    ApplicationStages = applicationStages.ToList()
                };
                
                return View(vm);
            }

            //Add error: loading application
            return RedirectToAction(nameof(OfferController.Index));
        }

        [HttpPost]
        [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        public async Task<IActionResult> DeleteApplication(string id, string returnUrl = null)
        {
            var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                throw new Exception($"Application with id: {id} doesn't exist.");
            }

            var delete = await _cvStorageService.DeleteCvAsync(application.CvFileName);
            if (!delete)
            {
                throw new Exception($"Something went wrong while deleting cv in Blob: {application.CvFileName}.");
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return RedirectToLocal(returnUrl);
        }

        [Authorize(Roles = RoleCollection.Administrator + "," + RoleCollection.Recruiter)]
        public async Task<ActionResult> ApplicationsViewHistory(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var application = _context.Applications.FirstOrDefault(x => x.Id == id);

            if (application != null)
            {
                var vm = await _context.ApplicationsViewHistories
                                            .Where(x => x.ApplicationId == application.Id)
                                            .OrderByDescending(x => x.ViewTime)
                                            .ToListAsync();
                foreach (var viewHistory in vm)
                    viewHistory.ViewTime = viewHistory.ViewTime.ToLocalTime();

                return View(vm);
            }

            //Add error: loading application Views
            return RedirectToAction(nameof(OfferController.Index));
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

        //[Authorize(Roles = RoleCollection.Recruit)]
        //public IActionResult MyApplications()
        //{
        //    var userId = _userManager.GetUserId(HttpContext.User);
        //    var applications = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).Where(x => x.UserId == userId);

        //    var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<MyApplicationsViewModel>>(applications);
        //    foreach (var application in vm)
        //        application.CreatedAt = application.CreatedAt.ToLocalTime();

        //    return View(vm);
        //}

        //[Authorize(Roles = RoleCollection.Recruit)]
        //public async Task<ActionResult> MyApplicationDetails(string id, string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;

        //    var application = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).Include(x => x.ApplicationStages).FirstOrDefault(x => x.Id == id);
        //    var userId = _userManager.GetUserId(HttpContext.User);

        //    if (application != null && userId == application.UserId )
        //    {
        //        await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            ViewTime = DateTime.UtcNow,
        //            ApplicationId = application.Id,
        //            UserId = _userManager.GetUserId(HttpContext.User)
        //        });
        //        await _context.SaveChangesAsync();

        //        var vm = new MyApplicationDetailsViewModel()
        //        {
        //            Id = application.Id,
        //            User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
        //            JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
        //            CvFileUrl = _cvStorageService.UriFor(application.CvFileName),
        //            CreatedAt = application.CreatedAt.ToLocalTime(),
        //            ApplicationViews = await _context.ApplicationsViewHistories
        //                                            .Where(x => x.ApplicationId == application.Id && x.UserId != userId)
        //                                            .CountAsync(),
        //            ApplicationViewsAll = await _context.ApplicationsViewHistories
        //                                            .Where(x => x.ApplicationId == application.Id)
        //                                            .CountAsync(),
        //            ApplicationStages = application.ApplicationStages
        //                                            .OrderBy(x => x.Level).ToList()
        //        };

        //        return View(vm);
        //    }

        //    //Add error: It's not yours application/cv 
        //    //redirect to my application list
        //    return RedirectToAction(nameof(OfferController.Index));
        //}

        //[HttpPost]
        //[Authorize(Roles = RoleCollection.Recruit)]
        //public async Task<IActionResult> DeleteMyApplication(string id)
        //{
        //    var userId = _userManager.GetUserId(HttpContext.User);
        //    var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == id);

        //    if (application == null)
        //    {
        //        throw new Exception($"Application with id: {id} doesn't exist.");
        //    }
        //    if (application.UserId != userId)
        //    {
        //        throw new Exception($"User with id: {userId} aren't owner of application with id: {application.Id}.");
        //    }

        //    var delete = await _cvStorageService.DeleteCvAsync(application.CvFileName);
        //    if (!delete)
        //    {
        //        throw new Exception($"Something went wrong while deleting cv in Blob: {application.CvFileName}.");
        //    }

        //    _context.Applications.Remove(application);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(ApplicationController.MyApplications));
        //}

        //[Authorize(Roles = RoleCollection.Recruit)]
        //public async Task<IActionResult> Apply(string id, string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;

        //    var offer = await _context.JobPositions.SingleOrDefaultAsync(x => x.Id == id);
        //    if (offer == null)
        //        return RedirectToAction(nameof(OfferController.Index));

        //    var vm = new ApplyApplicationViewModel()
        //    {
        //        JobPositionId = offer.Id,
        //        JobPositionName = offer.Name,
        //    };
        //    return View(vm);
        //}

        //[HttpPost]
        //[Authorize(Roles = RoleCollection.Recruit)]
        //public async Task<IActionResult> Apply(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel)
        //{
        //    if (!ModelState.IsValid)
        //        return View(applyApplicationViewModel);

        //    if (cv != null)
        //    {
        //        var userId = _userManager.GetUserId(HttpContext.User);
        //        using (var stream = cv.OpenReadStream())
        //        {
        //            var CvFileName = await _cvStorageService.SaveCvAsync(stream, userId, cv.FileName);
        //            applyApplicationViewModel.CvFileName = CvFileName;
        //        }

        //        if (Path.GetExtension(cv.FileName) != ".pdf")
        //        {
        //            ModelState.AddModelError("", "CV must have .pdf extension.");
        //            return View(applyApplicationViewModel);
        //        }
        //        if (applyApplicationViewModel.CvFileName == null)
        //        {
        //            ModelState.AddModelError("", "Something went wrong during uploading CV, try again or contact with admin.");
        //            return View(applyApplicationViewModel);
        //        }

        //        if (await _context.Applications
        //            .Where(x => x.UserId == userId && x.JobPositionId == applyApplicationViewModel.JobPositionId).CountAsync() != 0)
        //        {
        //            ModelState.AddModelError("", "You have already sent application to this offer.");
        //            return View(applyApplicationViewModel);
        //        }

        //        var application = new Application()
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            CvFileName = applyApplicationViewModel.CvFileName,
        //            JobPositionId = applyApplicationViewModel.JobPositionId,
        //            UserId = userId,
        //            CreatedAt = DateTime.UtcNow
        //        };
        //        await _context.Applications.AddAsync(application);
        //        //await _context.SaveChangesAsync();

        //        var applicationStagesRequirements = await _context.ApplicationStagesRequirements.FirstOrDefaultAsync(x => x.JobPositionId == application.JobPositionId);
        //        List<ApplicationStageBase> applicationStages = new List<ApplicationStageBase>();
        //        if(applicationStagesRequirements.IsApplicationApprovalRequired)
        //        {
        //            applicationStages.Add(new ApplicationApproval() {
        //                Id = Guid.NewGuid().ToString(),
        //                ApplicationId = application.Id,
        //                ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForApplicatioApprovalId
        //            });
        //        }
        //        if (applicationStagesRequirements.IsPhoneCallRequired)
        //        {
        //            applicationStages.Add(new PhoneCall()
        //            {
        //                Id = Guid.NewGuid().ToString(),
        //                ApplicationId = application.Id,
        //                ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForPhoneCallId
        //            });
        //        }
        //        if (applicationStagesRequirements.IsHomeworkRequired)
        //        {
        //            applicationStages.Add(new Homework()
        //            {
        //                Id = Guid.NewGuid().ToString(),
        //                ApplicationId = application.Id,
        //                ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForHomeworkId
        //            });
        //        }
        //        if (applicationStagesRequirements.IsInterviewRequired)
        //        {
        //            applicationStages.Add(new Interview()
        //            {
        //                Id = Guid.NewGuid().ToString(),
        //                ApplicationId = application.Id,
        //                ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForInterviewId
        //            });
        //        }

        //        await _context.ApplicationStages.AddRangeAsync(applicationStages);
        //        await _context.SaveChangesAsync();

        //        return RedirectToAction(nameof(ApplicationController.MyApplicationDetails), new { id = application.Id });
        //    }

        //    ModelState.AddModelError("", "CV file not found.");
        //    return View(applyApplicationViewModel);
        //}


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        //public IActionResult Add()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Add(IFormFile cv, AddApplicationViewModel addApplicationViewModel)
        //{
        //    if (!ModelState.IsValid)
        //        return View(addApplicationViewModel);

        //    if (cv != null)
        //    {
        //        var userId = _userManager.GetUserId(HttpContext.User);
        //        using (var stream = cv.OpenReadStream())
        //        {
        //            var pdfId = await cvStorage.SaveCv(stream, userId);
        //            addApplicationViewModel.CvId = pdfId;
        //            addApplicationViewModel.UserId = userId;

        //            //return RedirectToAction("Show", new { id = pdfId });
        //            return RedirectToAction(nameof(ApplicationController.Show), addApplicationViewModel);
        //        }
        //    }

        //    return View(addApplicationViewModel);
        //}
    }
}