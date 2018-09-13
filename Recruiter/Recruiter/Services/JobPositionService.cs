﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.JobPositionViewModels;
using Recruiter.Repositories;

namespace Recruiter.Services
{
    public class JobPositionService : IJobPositionService
    {
        private readonly IJobPositionRepository _jobPositionRepository;
        private readonly IMapper _mapper;

        public JobPositionService(IJobPositionRepository jobPositionRepository, IMapper mapper)
        {
            _jobPositionRepository = jobPositionRepository;
            _mapper = mapper;
        }
        public async Task<JobPositionViewModel> GetAsync(string id)
        {
            var jobPosition = await _jobPositionRepository.GetAsync(id);

            return _mapper.Map<JobPosition, JobPositionViewModel>(jobPosition);
        }

        public async Task<IEnumerable<JobPositionViewModel>> GetAllAsync()
        {
            var jobPositions = await _jobPositionRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<JobPosition>, IEnumerable<JobPositionViewModel>>(jobPositions);
        }

        public async Task<string> AddAsync(AddJobPositionViewModel addJobPositionViewModel)
        {
            var jobPosition = new JobPosition()
            {
                Id = Guid.NewGuid().ToString(),
                Name = addJobPositionViewModel.Name,
                Description = addJobPositionViewModel.Description
            };

            await _jobPositionRepository.AddAsync(jobPosition);
            return jobPosition.Id;
        }
    }
}
