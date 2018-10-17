using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class ApplicationsViewHistory
    {
        public string Id { get; set; }

        [Display(Name = "View Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime ViewTime { get; set; }

        [Display(Name = "User Id")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Application Id")]
        public string ApplicationId { get; set; }
        public virtual Application Application { get; set; }
    }
}
