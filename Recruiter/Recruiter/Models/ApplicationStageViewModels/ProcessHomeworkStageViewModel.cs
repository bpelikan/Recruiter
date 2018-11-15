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
        //public bool Accepted { get; set; }
        public string Note { get; set; }
        public int Rate { get; set; }

        public string Description { get; set; }
        public int Duration { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? StartTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndTime { get; set; }

        public string Url { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? SendingTime { get; set; }
    }
}
