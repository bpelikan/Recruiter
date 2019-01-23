using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Newtonsoft.Json;
using Recruiter.Models;

namespace Recruiter.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class InterviewAppointmentController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public InterviewAppointmentController(
                    ILogger<InterviewAppointmentController> logger,
                    ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //[HttpGet]
        //public JsonResult Index()
        //{
        //    return JsonResult("{\"Test\":\"True\"}");
        //}

        //[ProducesResponseType(typeof(bool), 200)]
        //[ProducesResponseType(404)]


        [HttpGet("{appointmentId}")]
        public IActionResult CheckAppointmentStatus(string appointmentId)
        {
            var appointment = _context.InterviewAppointments.FirstOrDefault(x => x.Id == appointmentId);

            if (appointment == null)
                return NotFound();
            if (appointment.InterviewAppointmentState == InterviewAppointmentState.Confirmed)
                return Ok();
            else
                return NotFound();
        }

    }
}