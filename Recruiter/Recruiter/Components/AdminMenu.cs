using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Components
{
    public class AdminMenu : ViewComponent
    {
        private readonly IStringLocalizer<AdminMenu> _stringLocalizer;

        public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public IViewComponentResult Invoke()
        {
            var menuItems = new List<AdminMenuItem> {
                new AdminMenuItem(){
                    DisplayValue = _stringLocalizer["User management"],
                    ActionValue = "UserManagement"
                },
                new AdminMenuItem(){
                    DisplayValue = _stringLocalizer["Role management"],
                    ActionValue = "RoleManagement"
                }};

            return View(menuItems);
        }
    }

    public class AdminMenuItem
    {
        public string DisplayValue { get; set; }
        public string ActionValue { get; set; }
    }
}
