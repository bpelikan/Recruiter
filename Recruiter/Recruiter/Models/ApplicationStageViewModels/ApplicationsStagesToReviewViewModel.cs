using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class ApplicationsStagesToReviewViewModel
    {
        public List<ApplicationViewModel> Applications { get; set; }

        public IEnumerable<StagesViewModel> Stages { get; set; }
    }

    public class ApplicationViewModel
    {
        public string Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }

        public virtual JobPositionViewModel JobPosition { get; set; }

        public virtual UserDetailsViewModel User { get; set; }
    }

    public class StagesViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
