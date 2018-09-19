﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.ApplicationViewModels
{
    public class ApplicationDetailsViewModel
    {
        public virtual UserDetailsViewModel User { get; set; }

        public virtual JobPositionViewModel JobPosition { get; set; }

        public string CvFileUrl { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }
    }
}
