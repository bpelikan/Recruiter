using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Models;
using Recruiter.Models.CvViewModels;
using Recruiter.Services;

namespace Recruiter.Controllers
{
    public class CvController : Controller
    {
        private readonly ICvStorage cvStorage;
        private readonly UserManager<ApplicationUser> _userManager;

        public CvController(ICvStorage cvStorage, UserManager<ApplicationUser> userManager)
        {
            this.cvStorage = cvStorage;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile cv)
        {
            if (cv != null)
            {
                var userId = _userManager.GetUserId(HttpContext.User);
                using (var stream = cv.OpenReadStream())
                {
                    var pdfId = await cvStorage.SaveCv(stream, userId);
                    return RedirectToAction("Show", new { id = pdfId });
                }
            }
            return View();
        }

        public ActionResult Show(string id)
        {
            var model = new ShowModel { Uri = cvStorage.UriFor(id) };
            return View(model);
        }
    }
}