using Microsoft.AspNetCore.Http;
using Recruiter.Models;
using Recruiter.Models.MyApplicationViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IMyApplicationService
    {
        IEnumerable<MyApplicationsViewModel> GetMyApplications(string userId);
        Task<MyApplicationDetailsViewModel> GetMyApplicationDetails(string applicationId, string userId);
        Task DeleteMyApplication(string applicationId, string userId);
        Task<ApplyApplicationViewModel> GetApplyApplicationViewModel(string jobPositionId, string userId);
        Task<Application> ApplyMyApplication(IFormFile cv, ApplyApplicationViewModel applyApplicationViewModel, string userId);
        Task<Homework> GetHomeworkStageToShowInProcessMyHomework(string stageId, string userId);
        Task<Homework> GetViewModelForBeforeReadMyHomework(string stageId, string userId);
        Task UpdateMyHomeworkAsReaded(string stageId, string userId);
        Task<ReadMyHomeworkViewModel> GetViewModelForReadMyHomework(string stageId, string userId);
        Task SendMyHomework(ReadMyHomeworkViewModel homework, string userId);
        Task<Homework> GetViewModelForShowMyHomework(string stageId, string userId);
        Task<Interview> GetViewModelForConfirmInterviewAppointments(string stageId, string userId);
        Task ConfirmAppointmentInInterview(string interviewAppointmentId, string userId);
        Task RequestForNewAppointmentsInInterview(string interviewId, string userId);
        Task<ScheduleInterviewAppointmentReminderViewModel> GetViewModelForScheduleInterviewAppointmentReminder(string interviewAppointmentId, string userId);
        Task ProcessScheduleInterviewAppointmentReminder(string interviewAppointmentId, int time, string userId);
    }
}
