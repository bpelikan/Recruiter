using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Recruiter.Data;
using Recruiter.Models.JobPositionViewModels;

namespace Recruiter.Controllers
{
    public class JobPositionController : Controller
    {

        private readonly ApplicationDbContext _context;

        public JobPositionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var jobpositions = _context.JobPositions;
            return View(jobpositions);
        }

        
    }
}