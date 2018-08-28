using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Data
{
    public static class DbIdentityInitializer
    {
        public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager, configuration);
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            if (userManager.FindByEmailAsync("admin@admin.com").Result == null)
            {
                var user = new ApplicationUser()
                {
                    Email = "admin@admin.com",
                    UserName = "admin@admin.com".Normalize().ToUpper(),
                    FirstName = "Admin",
                    LastName = "Admin"
                };

                IdentityResult result = userManager.CreateAsync(user, configuration["AdminPassword"]).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Administrator").Wait();
                }
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Administrator").Result)
            {
                IdentityRole role = new IdentityRole()
                {
                    Name = "Administrator"
                };
                IdentityResult roleResult = roleManager.CreateAsync(role).Result;
            }
        }
    }
}
