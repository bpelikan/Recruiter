using Recruiter.Models;
using Recruiter.Models.ApplicationStageViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public interface IApplicationStageService
    {
        //GET
        Task<IEnumerable<InterviewAppointment>> GetCollidingInterviewAppointment(InterviewAppointment interview, string userId);


        //GET ApplicationStageBase
        Task<ApplicationStageBase> GetApplicationStageBase(string stageId, string userId);
        Task<ApplicationStageBase> GetApplicationStageBaseToProcessStage(string stageId, string userId);
        Task<ApplicationStageBase> GetApplicationStageBaseWithIncludeNoTracking(string stageId, string userId);
        Task<ApplicationStageBase> GetApplicationStageBaseWithIncludeOtherStages(string stageId, string userId);
        Task<ApplicationStageBase> GetApplicationStageBaseToShowInProcessStage(string stageId, string userId);


        //GET ViewModelFor
        ApplicationsStagesToReviewViewModel GetViewModelForApplicationsStagesToReview(string stageName, string userId);
        Task<AssingUserToStageViewModel> GetViewModelForAssingUserToStage(string stageId, string userId);
        Task<ProcessApplicationApprovalViewModel> GetViewModelForProcessApplicationApproval(string stageId, string userId);
        Task<ProcessPhoneCallViewModel> GetViewModelForProcessPhoneCall(string stageId, string userId);
        Task<AddHomeworkSpecificationViewModel> GetViewModelForAddHomeworkSpecification(string stageId, string userId);
        Task<ProcessHomeworkStageViewModel> GetViewModelForProcessHomeworkStage(string stageId, string userId);
        Task<SetAppointmentsToInterviewViewModel> GetViewModelForSetAppointmentsToInterview(string stageId, string userId);
        Task<ProcessInterviewViewModel> GetViewModelForProcessInterviewStage(string stageId, string userId);
        Task<IEnumerable<InterviewAppointment>> GetViewModelForShowAssignedAppointments(string userId);


        //ADD
        Task<bool> AddRequiredStagesToApplication(string applicationId);

        Task AddNewInterviewAppointments(SetAppointmentsToInterviewViewModel setAppointmentsToInterviewViewModel, string userId);


        //UPDATE
        Task<bool> UpdateNextApplicationStageState(string applicationId, string userId);
        Task UpdateResponsibleUserInApplicationStage(AssingUserToStageViewModel addResponsibleUserToStageViewModel, string userId);
        Task UpdateApplicationApprovalStage(ProcessApplicationApprovalViewModel applicationApprovalViewModel, bool accepted, string userId);
        Task UpdatePhoneCallStage(ProcessPhoneCallViewModel phoneCallViewModel, bool accepted, string userId);
        Task UpdateHomeworkSpecification(AddHomeworkSpecificationViewModel addHomeworkSpecificationViewModel, string userId);
        Task UpdateHomeworkStage(ProcessHomeworkStageViewModel processHomeworkStageViewModel, bool accepted, string userId);
        Task UpdateInterviewStage(ProcessInterviewViewModel interviewViewModel, bool accepted, string userId);


        //REMOVE
        Task<InterviewAppointment> RemoveAssignedAppointment(string appointmentId, string userId);
        Task<InterviewAppointment> RemoveAppointmentsFromInterview(string appointmentId, string userId);
        

        //SEND
        Task SendInterviewAppointmentsToConfirm(string stageId, bool accepted, string userId);

    }
}
