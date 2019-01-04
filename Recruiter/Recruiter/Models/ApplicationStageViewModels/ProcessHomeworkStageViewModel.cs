using Recruiter.Models.ApplicationStageViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class ProcessHomeworkStageViewModel
    {
        public ApplicationViewModel Application { get; set; }

        public virtual ICollection<ApplicationStageBase> ApplicationStagesFinished { get; set; }
        public HomeworkViewModel StageToProcess { get; set; }
        public virtual ICollection<ApplicationStageBase> ApplicationStagesWaiting { get; set; }
    }
    public class HomeworkViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Rate")]
        [Range(1, 5, ErrorMessage = "Rate must be beetween 1 and 5")]
        public int Rate { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }
        [Display(Name = "Duration")]
        public int Duration { get; set; }

        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Url")]
        public string Url { get; set; }

        [Display(Name = "Sending Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? SendingTime { get; set; }
    }
}
