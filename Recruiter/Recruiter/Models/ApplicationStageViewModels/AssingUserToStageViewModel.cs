using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationStageViewModels
{
    public class AssingUserToStageViewModel
    {
        public string ApplicationId { get; set; }

        public string StageId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
