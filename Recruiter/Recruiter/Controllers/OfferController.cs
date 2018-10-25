using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruiter.Models;
using Recruiter.Models.OfferViewModel;
using Recruiter.Repositories;

namespace Recruiter.Controllers
{
    public class OfferController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IMapper _mapper;

        public OfferController(UserManager<ApplicationUser> userManager, IJobPositionRepository jobPositionRepository, IMapper mapper)
        {
            _userManager = userManager;
            _jobPositionRepository = jobPositionRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var jobPositions = await _jobPositionRepository.GetAllAsync();
            jobPositions = jobPositions.Where(x => (x.EndDate >= DateTime.UtcNow || x.EndDate == null) && x.StartDate <= DateTime.UtcNow);
            var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<OfferViewModel>>(jobPositions);

            return View(vm);
        }

        public async Task<IActionResult> Details(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var offer = await _jobPositionRepository.GetAsync(id);
            if (offer == null)
                return RedirectToAction(nameof(OfferController.Index));

            var vm = _mapper.Map<JobPosition, OfferDetailsViewModel>(offer);
            return View(vm);
        }
    }
}