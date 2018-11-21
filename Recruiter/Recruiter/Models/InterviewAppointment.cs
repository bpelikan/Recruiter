using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models
{
    public class InterviewAppointment
    {
        public string Id { get; set; }

        //public string RecruitId { get; set; }
        //public virtual ApplicationUser Recruit { get; set; }

        //public string RecruiterId { get; set; }
        //public virtual ApplicationUser Recruiter { get; set; }
        
        public string InterviewId { get; set; }
        public virtual Interview Interview { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime StartTime { get; set; }

        public int Duration { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime EndTime { get; set; }

        public bool AcceptedByRecruit { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime? AcceptedByRecruitTime { get; set; }
    }
}
