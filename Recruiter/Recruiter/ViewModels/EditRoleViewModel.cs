using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.ViewModels
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Role name")]
        [Required(ErrorMessage = "{0} is required")]
        public string RoleName { get; set; }

        public List<string> Users { get; set; }
    }
}
