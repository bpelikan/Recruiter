using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Models.AdminViewModels
{
    public class UserDetailsWithRolesViewModel
    {
        public UserDetailsWithRolesViewModel()
        {
            Roles = new List<RoleInUserDetails>();
        }

        public UserDetailsViewModel User { get; set; }

        //roles
        public List<RoleInUserDetails> Roles { get; }

        public void AddRole(string name, bool isInRole)
        {
            Roles.Add(new RoleInUserDetails()
            {
                Name = name,
                IsInRole = isInRole
            });
        }

        public class RoleInUserDetails
        {
            public string Name { get; set; }
            public bool IsInRole { get; set; }
        }
    }

    
}
