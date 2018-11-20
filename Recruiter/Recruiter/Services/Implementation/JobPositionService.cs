using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Shared;

namespace Recruiter.Services.Implementation
{
    public class JobPositionService : IJobPositionService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public JobPositionService(
                    IMapper mapper,
                    ILogger<JobPositionService> logger,
                    ApplicationDbContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public IEnumerable<JobPositionViewModel> GetViewModelForIndexByJobPositionActivity(string jobPositionActivity, string userId)
        {
            IEnumerable<JobPosition> jobPositions = null;
            switch (jobPositionActivity)
            {
                case JobPositionActivity.Active:
                    jobPositions = _context.JobPositions.Where(x => x.StartDate <= DateTime.UtcNow &&
                                                                    (x.EndDate >= DateTime.UtcNow || x.EndDate == null))
                                                        .OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
                    break;

                case JobPositionActivity.Planned:
                    jobPositions = _context.JobPositions.Where(x => x.StartDate > DateTime.UtcNow)
                                                        .OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
                    break;

                case JobPositionActivity.Expired:
                    jobPositions = _context.JobPositions.Where(x => x.EndDate < DateTime.UtcNow)
                                                        .OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
                    break;

                default:
                    jobPositions = _context.JobPositions.OrderByDescending(x => x.EndDate == null).ThenByDescending(x => x.EndDate);
                    break;
            }

            var vm = _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);

            foreach (var jobPosition in vm)
            {
                jobPosition.StartDate = jobPosition.StartDate.ToLocalTime();
                jobPosition.EndDate = jobPosition.EndDate?.ToLocalTime();
            }

            return vm;

            //throw new NotImplementedException();
        }
    }
}
