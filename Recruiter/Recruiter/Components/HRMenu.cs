using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Components
{
    public class HRMenu : ViewComponent
    {
        private readonly IStringLocalizer<HRMenu> _stringLocalizer;

        public HRMenu(IStringLocalizer<HRMenu> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public IViewComponentResult Invoke()
        {
            var menuItems = new List<HRMenuItem> { new HRMenuItem()
                {
                    DisplayValue = _stringLocalizer["Applications management"],
                    ControllerValue = "Application",
                    ActionValue = "Applications"

                },
                new HRMenuItem()
                {
                    DisplayValue = _stringLocalizer["Job Positions management"],
                    ControllerValue = "JobPosition",
                    ActionValue = "Index"
                }};

            return View(menuItems);
        }
    }

    public class HRMenuItem
    {
        public string DisplayValue { get; set; }
        public string ControllerValue { get; set; }
        public string ActionValue { get; set; }
    }
}
