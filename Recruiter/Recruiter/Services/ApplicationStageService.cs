using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public class ApplicationStageService : IApplicationStageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ApplicationStageService(ApplicationDbContext context, ILogger<ApplicationStageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> UpdateNextApplicationStageState(string applicationId)
        {
            _logger.LogInformation($"Executing UpdateNextApplicationStageState with applicationId={applicationId}");

            var application = await _context.Applications
                                                .Include(x => x.ApplicationStages)
                                                .FirstOrDefaultAsync(x => x.Id == applicationId);
            if (application == null)
                throw new Exception($"Application with id {applicationId} not found.)");

            if (application.ApplicationStages.Count() != 0)
            {
                var nextStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State != ApplicationStageState.Finished).FirstOrDefault();
                var prevStage = application.ApplicationStages.OrderBy(x => x.Level).Where(x => x.State == ApplicationStageState.Finished).Last();

                if (nextStage != null && nextStage.State == ApplicationStageState.Waiting)
                {
                    if (prevStage.Accepted)
                    {
                        nextStage.State = ApplicationStageState.InProgress;
                    }
                    else
                    {
                        foreach (var stage in application.ApplicationStages.Where(x => x.State != ApplicationStageState.Finished))
                        {
                            stage.Accepted = false;
                            stage.State = ApplicationStageState.Finished;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> AddRequiredStagesToApplication(string applicationId)
        {
            _logger.LogInformation($"Executing AddStagesToApplication with applicationId={applicationId}");

            var application = _context.Applications.FirstOrDefault(x => x.Id == applicationId);
            if (application == null)
                throw new Exception($"Application with id: {applicationId} not found.");

            var applicationStagesRequirements = await _context.ApplicationStagesRequirements.FirstOrDefaultAsync(x => x.JobPositionId == application.JobPositionId);
            if (applicationStagesRequirements == null)
                throw new Exception($"Application Stages Requirements with id: {application.JobPositionId} not found.");

            List<ApplicationStageBase> applicationStages = new List<ApplicationStageBase>();
            if (applicationStagesRequirements.IsApplicationApprovalRequired)
            {
                applicationStages.Add(new ApplicationApproval()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForApplicatioApprovalId
                });
            }
            if (applicationStagesRequirements.IsPhoneCallRequired)
            {
                applicationStages.Add(new PhoneCall()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForPhoneCallId
                });
            }
            if (applicationStagesRequirements.IsHomeworkRequired)
            {
                applicationStages.Add(new Homework()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForHomeworkId,
                    HomeworkState = HomeworkState.WaitingForSpecification
                });
            }
            if (applicationStagesRequirements.IsInterviewRequired)
            {
                applicationStages.Add(new Interview()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApplicationId = application.Id,
                    ResponsibleUserId = applicationStagesRequirements.DefaultResponsibleForInterviewId
                });
            }

            if (applicationStages.OrderBy(x => x.Level).First().ResponsibleUserId != null)
                applicationStages.OrderBy(x => x.Level).First().State = ApplicationStageState.InProgress;

            await _context.ApplicationStages.AddRangeAsync(applicationStages);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ApplicationStageBase> GetApplicationStageBaseToShowInProcessStage(string stageId, string userId)
        {
            var stage = await _context.ApplicationStages
                                    //.Include(x => x.Application)
                                    //    .ThenInclude(x => x.ApplicationStages)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.User)
                                    .Include(x => x.Application)
                                        .ThenInclude(x => x.JobPosition)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == stageId);
            if (stage == null)
                throw new Exception($"ApplicationStage with id {stageId} not found. (UserID: {userId})");
            if (stage.ResponsibleUserId != userId)
                throw new Exception($"User with ID: {userId} is not responsible user of ApplicationStage with ID: {stage.Id}. (UserID: {userId})");

            return stage;
        }
    }
}
