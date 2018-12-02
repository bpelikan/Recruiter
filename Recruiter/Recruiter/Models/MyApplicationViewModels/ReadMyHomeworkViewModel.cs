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

        public string Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? StartTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? EndTime { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }
    }
}
