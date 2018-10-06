using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recruiter.Models;

namespace Recruiter.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult About()
        //{
        //    ViewData["Message"] = "Your application description page.";

        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        public IActionResult Test(string id = null)
        {
            throw new Exception($"Test function with exception id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
        }

        public IActionResult LoggerTest(string id = null)
        {
            _logger.LogCritical     ($"LoggerTest-LogCritical id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            _logger.LogDebug        ($"LoggerTest-LogDebug id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            _logger.LogError        ($"LoggerTest-LogError id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            _logger.LogInformation  ($"LoggerTest-LogInformation id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            _logger.LogTrace        ($"LoggerTest-LogTrace id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            _logger.LogWarning      ($"LoggerTest-LogWarning id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
            throw new Exception($"LoggerTest function with exception id: {id}. (UserID: {_userManager.GetUserId(HttpContext.User)})");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
