using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Models;
using Recruiter.Models.ApplicationViewModels;
using Recruiter.Services;

namespace Recruiter.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly ICvStorage cvStorage;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationController(ICvStorage cvStorage, UserManager<ApplicationUser> userManager)
        {
            this.cvStorage = cvStorage;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(IFormFile cv, AddApplicationViewModel addApplicationViewModel)
        {
            if (!ModelState.IsValid)
                return View(addApplicationViewModel);

            if (cv != null)
            {
                using (var stream = cv.OpenReadStream())
                {
                    var pdfId = await cvStorage.SaveCv(stream);
                    addApplicationViewModel.CvId = pdfId;

                    addApplicationViewModel.UserId = _userManager.GetUserId(HttpContext.User);

                    //return RedirectToAction("Show", new { id = pdfId });
                    return RedirectToAction(nameof(ApplicationController.Show), addApplicationViewModel);
                }
            }

            return View(addApplicationViewModel);
        }

        public ActionResult Show(AddApplicationViewModel addApplicationViewModel)
        {
            var vm = new ShowApplicationViewModel()
            {
                Name = addApplicationViewModel.Name,
                Description = addApplicationViewModel.Description,
                UserId = addApplicationViewModel.UserId,
                CvUri = cvStorage.UriFor(addApplicationViewModel.CvId)
            };

            return View(vm);
        }
    }
}