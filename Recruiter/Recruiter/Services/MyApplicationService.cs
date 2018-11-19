using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;

namespace Recruiter.Services
{
    public class MyApplicationService : IMyApplicationService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public MyApplicationService(IMapper mapper, ILogger<MyApplicationService> logger, ApplicationDbContext context)
        {
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public IEnumerable<MyApplicationsViewModel> GetViewModelForMyApplications(string userId)
        {
            _logger.LogInformation($"Executing GetViewModelForMyApplications. (UserID: {userId})");

            var applications = _context.Applications
                                        .Include(x => x.JobPosition)
                                        .Include(x => x.User)
                                        .Where(x => x.UserId == userId);

            var vm = _mapper.Map<IEnumerable<Application>, IEnumerable<MyApplicationsViewModel>>(applications);
            foreach (var application in vm)
                application.CreatedAt = application.CreatedAt.ToLocalTime();

            return vm;
        }
    }
}
