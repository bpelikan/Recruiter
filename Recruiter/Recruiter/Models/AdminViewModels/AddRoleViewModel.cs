using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.AdminViewModels
{
    public class AddRoleViewModel
    {
        [Display(Name = "Role name")]
        [Required(ErrorMessage = "{0} is required")]
        public string RoleName { get; set; }
    }
}
