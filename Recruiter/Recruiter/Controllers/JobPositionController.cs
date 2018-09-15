using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Repositories;

namespace Recruiter.Controllers
{
    public class JobPositionController : Controller
    {
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IMapper _mapper;

        public JobPositionController(IJobPositionRepository jobPositionRepository, IMapper mapper)
        {
            _jobPositionRepository = jobPositionRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var jobPositions = await _jobPositionRepository.GetAllAsync();
            var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);

            return View(vm);
        }

        public async Task<IActionResult> Details(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var jobPosition = await _jobPositionRepository.GetAsync(id);
            if (jobPosition == null)
                return RedirectToAction(nameof(JobPositionController.Index));

            var vm = _mapper.Map<JobPosition, JobPositionViewModel>(jobPosition);

            return View(vm);
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

            var jobPosition = new JobPosition()
            {
                Id = Guid.NewGuid().ToString(),
                Name = addJobPositionViewModel.Name,
                Description = addJobPositionViewModel.Description
            };

            await _jobPositionRepository.AddAsync(jobPosition);
            var jobPositionId = jobPosition.Id;

            if (jobPositionId != null)
                return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPositionId });

            ModelState.AddModelError("", "Something went wrong while adding this job position.");

            return View(addJobPositionViewModel);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var jobPosition = await _jobPositionRepository.GetAsync(id);

            if (jobPosition == null)
            {
                ModelState.AddModelError("", "Something went wrong while getting job position for editing.");
                return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
            }

            var vm = _mapper.Map<JobPosition, EditJobPositionViewModel>(jobPosition);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditJobPositionViewModel editJobPositionViewModel)
        {
            if (!ModelState.IsValid)
                return View(editJobPositionViewModel);

            var jobPosition = await _jobPositionRepository.GetAsync(editJobPositionViewModel.Id);

            if (jobPosition != null)
            {
                jobPosition.Name = editJobPositionViewModel.Name;
                jobPosition.Description = editJobPositionViewModel.Description;

                await _jobPositionRepository.UpdateAsync(jobPosition);

                return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPosition.Id });
            }

            ModelState.AddModelError("", "Something went wrong while editing job position.");
            return View(editJobPositionViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var jobPosition = await _jobPositionRepository.GetAsync(id);

            if (jobPosition != null)
            {
                await _jobPositionRepository.RemoveAsync(jobPosition);
                return RedirectToAction(nameof(JobPositionController.Index));
            }

            ModelState.AddModelError("", "Something went wrong while deleting this user.");
            return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
        }
    }
}