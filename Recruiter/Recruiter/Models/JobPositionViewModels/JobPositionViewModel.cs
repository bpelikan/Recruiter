using Recruiter.Models.AdminViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.JobPositionViewModels
{
    public class JobPositionViewModel
    {
        public JobPositionViewModel()
        {
            Applications = new List<ApplicationsViewModel>();
        }

        public string Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndDate { get; set; }

        public string CreatorId { get; set; }

        [Display(Name = "Creator")]
        public virtual UserDetailsViewModel Creator { get; set; }

        [Display(Name = "Application Stages Requirement")]
        public virtual ApplicationStagesRequirement ApplicationStagesRequirement { get; set; }


        public List<ApplicationsViewModel> Applications { get; }

        public void AddApplication(Application application)
        {
            Applications.Add(new ApplicationsViewModel()
            {
                Id = application.Id,
                CreatedAt = application.CreatedAt,
                User = new UserDetailsViewModel() {
                    FirstName = application.User.FirstName,
                    LastName = application.User.LastName
                }
            });
        }

        public class ApplicationsViewModel
        {
            public string Id { get; set; }

            [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
            public DateTime CreatedAt { get; set; }

            public virtual UserDetailsViewModel User { get; set; }
        }

        public class UserDetailsViewModel
        {
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Display(Name = "Last name")]
            public string LastName { get; set; }
        }
    }
}
