using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.MyApplicationViewModels
{
    public class ReadMyHomeworkViewModel
    {
        public string Id { get; set; }

        public string ApplicationId { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndTime { get; set; }

        [Required]
        [Display(Name = "Url")]
        [Url]
        public string Url { get; set; }
    }
}
