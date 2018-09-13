using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Services;

namespace Recruiter.Controllers
{
    public class JobPositionController : Controller
    {
        private readonly IJobPositionService _jobPositionService;
        private readonly IMapper _mapper;

        public JobPositionController(IJobPositionService jobPositionService, IMapper mapper)
        {
            _jobPositionService = jobPositionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var jobpositions = await _jobPositionService.GetAllAsync();
            //var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobpositions);
            return View(jobpositions);
        }

        public async Task<IActionResult> Details(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var jobposition = await _jobPositionService.GetAsync(id);
            if (jobposition == null)
                return RedirectToAction(nameof(JobPositionController.Index));

            return View(jobposition);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddJobPositionViewModel addJobPositionViewModel)
        {
            if (!ModelState.IsValid)
                return View(addJobPositionViewModel);

            var jobPositionId = await _jobPositionService.AddAsync(addJobPositionViewModel);

            if(jobPositionId != null)
                return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPositionId });

            return RedirectToAction(nameof(JobPositionController.Index));
            //return View(addJobPositionViewModel);
        }
    }
}