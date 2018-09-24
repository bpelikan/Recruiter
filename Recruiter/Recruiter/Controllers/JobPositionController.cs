using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Models;
using Recruiter.Models.AdminViewModels;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Repositories;

namespace Recruiter.Controllers
{
    [Authorize(Roles = "Recruiter, Administrator")]
    public class JobPositionController : Controller
    {
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobPositionController(UserManager<ApplicationUser> userManager, IJobPositionRepository jobPositionRepository, IMapper mapper)
        {
            _userManager = userManager;
            _jobPositionRepository = jobPositionRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var jobPositions = await _jobPositionRepository.GetAllAsync();
            var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);

            return View(vm);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var jobPosition = await _jobPositionRepository.GetAsync(id);
            //jobPosition.Creator = _mapper.Map<ApplicationUser, UserDetailsViewModel>(jobPosition.Creator);

            if (jobPosition == null)
                return RedirectToAction(nameof(JobPositionController.Index));

            var vm = _mapper.Map<JobPosition, JobPositionViewModel>(jobPosition);

            return View(vm);
        }

        public IActionResult Add()
        {
            var vm = new AddJobPositionViewModel()
            {
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 00),
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddJobPositionViewModel addJobPositionViewModel)
        {
            if (!ModelState.IsValid)
                return View(addJobPositionViewModel);

            var userId = _userManager.GetUserId(HttpContext.User);
            var jobPosition = new JobPosition()
            {
                Id = Guid.NewGuid().ToString(),
                Name = addJobPositionViewModel.Name,
                Description = addJobPositionViewModel.Description,
                StartDate = addJobPositionViewModel.StartDate,
                EndDate = addJobPositionViewModel.EndDate,
                CreatorId = userId
            };

            await _jobPositionRepository.AddAsync(jobPosition);
            var jobPositionId = jobPosition.Id;

            if (jobPositionId != null)
                return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPositionId });

            ModelState.AddModelError("", "Something went wrong while adding this job position.");

            //throw new Exception()
            return View(addJobPositionViewModel);
        }

        public async Task<IActionResult> Edit(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var jobPosition = await _jobPositionRepository.GetAsync(id);

            if (jobPosition == null)
            {
                throw new Exception($"Job position with id {id} not found.");
                //ModelState.AddModelError("", "Something went wrong while getting job position for editing.");
                //return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
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
                jobPosition.StartDate = editJobPositionViewModel.StartDate;
                jobPosition.EndDate = editJobPositionViewModel.EndDate;

                await _jobPositionRepository.UpdateAsync(jobPosition);

                return RedirectToAction(nameof(JobPositionController.Details), new { id = jobPosition.Id });
            }

            throw new Exception($"Job position with id {editJobPositionViewModel.Id} not found.");
            //ModelState.AddModelError("", "Something went wrong while editing job position.");
            //return View(editJobPositionViewModel);
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

            throw new Exception($"Job position with id {id} not found.");
            //ModelState.AddModelError("", "Something went wrong while deleting this user.");
            //return View(nameof(JobPositionController.Index), _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(await _jobPositionRepository.GetAllAsync()));
        }
    }
}