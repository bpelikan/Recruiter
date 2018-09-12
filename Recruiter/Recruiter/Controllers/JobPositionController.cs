using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Data;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Services;

namespace Recruiter.Controllers
{
    public class JobPositionController : Controller
    {
        private readonly IJobPositionService _jobPositionService;

        public JobPositionController(IJobPositionService jobPositionService)
        {
            _jobPositionService = jobPositionService;
        }

        public async Task<IActionResult> Index()
        {
            var jobpositions = await _jobPositionService.GetAllAsync();
            return View(jobpositions);
        }
    }
}