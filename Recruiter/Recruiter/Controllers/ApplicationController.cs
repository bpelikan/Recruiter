﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Recruiter.Services;

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

        [Authorize(Roles = "Recruiter, Administrator")]
        public IActionResult Applications()
        {
            var applications = _context.Applications.Include(x => x.JobPosition).Include(x => x.User);
            var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<ApplicationsViewModel>>(applications);

            return View(vm);
        }

        [Authorize(Roles = "Recruiter, Administrator")]
        public async Task<ActionResult> ApplicationDetails(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var application = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).FirstOrDefault(x => x.Id == id);

            if (application != null)
            {
                await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    ViewTime = DateTime.Now,
                    ApplicationId = application.Id,
                    UserId = _userManager.GetUserId(HttpContext.User)
                });
                await _context.SaveChangesAsync();

                var vm = new ApplicationDetailsViewModel()
                {
                    Id = application.Id,
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                    CvFileUrl = _cvStorageService.UriFor(application.CvFileName),
                    CreatedAt = application.CreatedAt,
                    ApplicationsViewHistories = await _context.ApplicationsViewHistories
                                                        .Where(x => x.ApplicationId == application.Id)
                                                        .OrderByDescending(x => x.ViewTime)
                                                        .Take(20)
                                                        .ToListAsync()
                };

                return View(vm);
            }

            //Add error: loading application
            return RedirectToAction(nameof(OfferController.Index));
        }

        [Authorize(Roles = "Recruiter, Administrator")]
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

                return View(vm);
            }

            //Add error: loading application Views
            return RedirectToAction(nameof(OfferController.Index));
        }


        [Authorize(Roles = "Recruit")]
        public IActionResult MyApplications()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var applications = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).Where(x => x.UserId == userId);

            var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<MyApplicationsViewModel>>(applications);

            return View(vm);
        }

        [Authorize(Roles = "Recruit")]
        public async Task<ActionResult> MyApplicationDetails(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var application = _context.Applications.Include(x => x.JobPosition).Include(x => x.User).FirstOrDefault(x => x.Id == id);
            var userId = _userManager.GetUserId(HttpContext.User);

            if (application != null && userId == application.UserId )
            {
                await _context.ApplicationsViewHistories.AddAsync(new ApplicationsViewHistory()
                {
                    Id = Guid.NewGuid().ToString(),
                    ViewTime = DateTime.Now,
                    ApplicationId = application.Id,
                    UserId = _userManager.GetUserId(HttpContext.User)
                });
                await _context.SaveChangesAsync();

                var vm = new MyApplicationDetailsViewModel()
                {
                    Id = application.Id,
                    User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(application.User),
                    JobPosition = _mapper.Map<JobPosition, JobPositionViewModel>(application.JobPosition),
                    CvFileUrl = _cvStorageService.UriFor(application.CvFileName),
                    CreatedAt = application.CreatedAt,
                    ApplicationViews = await _context.ApplicationsViewHistories
                                                    .Where(x => x.ApplicationId == application.Id && x.UserId != userId)
                                                    .CountAsync(),
                    ApplicationViewsAll = await _context.ApplicationsViewHistories
                                                    .Where(x => x.ApplicationId == application.Id)
                                                    .CountAsync()
                };

                return View(vm);
            }

            //Add error: It's not yours application/cv 
            //redirect to my application list
            return RedirectToAction(nameof(OfferController.Index));
        }

        [HttpPost]
        [Authorize(Roles = "Recruit")]
        public async Task<IActionResult> DeleteMyApplication(string id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var application = await _context.Applications.SingleOrDefaultAsync(x => x.Id == id);

            if (application == null)
            {
                throw new Exception($"Application with id: {id} doesn't exist.");
            }
            if (application.UserId != userId)
            {
                throw new Exception($"User with id: {userId} aren't owner of application with id: {application.Id}.");
            }
            
            var delete = await _cvStorageService.DeleteCvAsync(application.CvFileName);
            if (!delete)
            {
                throw new Exception($"Something went wrong while deleting cv in Blob: {application.CvFileName}.");
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ApplicationController.MyApplications));
        }

        [Authorize(Roles = "Recruit")]
        public async Task<IActionResult> Apply(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var offer = await _context.JobPositions.SingleOrDefaultAsync(x => x.Id == id);
            if (offer == null)
                return RedirectToAction(nameof(OfferController.Index));

            var vm = new ApplyApplicationViewModel()
            {
                JobPositionId = offer.Id,
                JobPositionName = offer.Name,
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Recruit")]
        public async Task<IActionResult> Apply(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel)
        {
            if (!ModelState.IsValid)
                return View(applyApplicationViewModel);

            if (cv != null)
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                using (var stream = cv.OpenReadStream())
                {
                    var CvFileName = await _cvStorageService.SaveCvAsync(stream, userId, cv.FileName);
                    applyApplicationViewModel.CvFileName = CvFileName;
                }

                if (Path.GetExtension(cv.FileName) != ".pdf")
                {
                    ModelState.AddModelError("", "CV must have .pdf extension.");
                    return View(applyApplicationViewModel);
                }

                if (await _context.Applications
                    .Where(x => x.UserId == userId && x.JobPositionId == applyApplicationViewModel.JobPositionId).CountAsync() != 0)
                {
                    ModelState.AddModelError("", "You have already sent application to this offer.");
                    return View(applyApplicationViewModel);
                }

                var application = new Application()
                {
                    Id = Guid.NewGuid().ToString(),
                    CvFileName = applyApplicationViewModel.CvFileName,
                    JobPositionId = applyApplicationViewModel.JobPositionId,
                    UserId = userId,
                    CreatedAt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second)
                };
                await _context.Applications.AddAsync(application);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ApplicationController.MyApplicationDetails), new { id = application.Id });
            }

            ModelState.AddModelError("", "CV file not found.");
            return View(applyApplicationViewModel);
        }


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