using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Recruiter.Models;
using Recruiter.Shared;
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
            SeedUsers(userManager, configuration["AdminEmail"], configuration["AdminPassword"]);
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, string adminEmail, string adminPassword)
        {
            if (adminEmail != null && adminPassword != null && userManager.FindByEmailAsync(adminEmail).Result == null)
            {
                var user = new ApplicationUser()
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    FirstName = "Admin",
                    LastName = "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                IdentityResult result = userManager.CreateAsync(user, adminPassword).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, RoleCollection.Administrator).Wait();
                }
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in RoleCollection.Roles)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    IdentityRole role = new IdentityRole()
                    {
                        Name = roleName
                    };
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }
        }
    }
}
