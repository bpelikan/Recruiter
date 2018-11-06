using Recruiter.Models.ApplicationStageViewModels.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class ApplicationsStagesToReviewViewModel
    {
        public List<AsignedStagesViewModel> AsignedStages { get; set; }

        public IEnumerable<StagesViewModel> StageSortedByName { get; set; }
    }

    public class AsignedStagesViewModel
    {
        public ApplicationViewModel Application { get; set; }
        public ApplicationStageBase CurrentStage { get; set; }
    }

    public class StagesViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
