using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Models.CvViewModels;
using Recruiter.Services;

namespace Recruiter.Controllers
{
    public class CvController : Controller
    {
        private readonly ICvStorage cvStorage;

        public CvController(ICvStorage cvStorage)
        {
            this.cvStorage = cvStorage;
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
                using (var stream = cv.OpenReadStream())
                {
                    var pdfId = await cvStorage.SaveCv(stream);
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