using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.AdminViewModels
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<UserDetails>();
        }

        public string Id { get; set; }

        [Display(Name = "Role name")]
        [Required(ErrorMessage = "{0} is required")]
        public string RoleName { get; set; }

        //users
        [Display(Name = "Users")]
        public List<UserDetails> Users { get; }

        public void AddUser(ApplicationUser user)
        {
            Users.Add(new UserDetails()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            });
        }

        public class UserDetails
        {
            public string Id { get; set; }

            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "First and last name")]
            public string FirstAndLastName
            {
                get { return FirstName + " " + LastName; }
            }
        }
    }
}
