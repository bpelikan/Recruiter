using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Recruiter.Models;
using Recruiter.Models.AdminViewModels;

namespace Recruiter.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<AdminController> _stringLocalizer;
        private readonly IMapper _mapper;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AdminController> logger, IStringLocalizer<AdminController> stringLocalizer, IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _stringLocalizer = stringLocalizer;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region UserManagement
        public IActionResult UserManagement()
        {
            var users = _userManager.Users.OrderBy(x => x.FirstAndLastName);
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction(nameof(AdminController.UserManagement));

            var vm = _mapper.Map<ApplicationUser, UserDetailsViewModel>(user);

            return View(vm);
        }

        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserViewModel addUserViewModel)
        {
            if (!ModelState.IsValid)
                return View(addUserViewModel);

            var user = new ApplicationUser()
            {
                UserName = addUserViewModel.Email.Normalize().ToUpper(),
                Email = addUserViewModel.Email,
                FirstName = addUserViewModel.FirstName,
                LastName = addUserViewModel.LastName,
                PhoneNumber = addUserViewModel.PhoneNumber,
                CreatedAt = DateTime.Now
            };

            IdentityResult result = await _userManager.CreateAsync(user, addUserViewModel.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User added successfully.");
                return RedirectToAction(nameof(AdminController.UserManagement));
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(addUserViewModel);
        }

        public async Task<IActionResult> EditUser(string id, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction(nameof(AdminController.UserManagement));

            var vm = _mapper.Map<ApplicationUser, EditUserViewModel>(user);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel editUserViewModel, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(editUserViewModel);

            var user = await _userManager.FindByIdAsync(editUserViewModel.Id);

            if (user != null)
            {
                user.UserName = editUserViewModel.Email.Normalize().ToUpper();
                user.Email = editUserViewModel.Email;
                user.FirstName = editUserViewModel.FirstName;
                user.LastName = editUserViewModel.LastName;
                user.PhoneNumber = editUserViewModel.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User changed successfully.");
                    //return RedirectToAction(nameof(AdminController.UserDetails), "Admin", new { id = user.Id });
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError("", _stringLocalizer["User not updated, something went wrong."]);

                return View(user);
            }

            return RedirectToAction(nameof(AdminController.UserManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully.");
                    return RedirectToAction(nameof(AdminController.UserManagement));
                }
                else
                {
                    ModelState.AddModelError("", _stringLocalizer["Something went wrong while deleting this user."]);
                }
            }
            else
            {
                ModelState.AddModelError("", _stringLocalizer["This user can't be found."]);
            }
            return View(nameof(AdminController.UserManagement), _userManager.Users);
        }
        #endregion

        #region RoleManagement
        public IActionResult RoleManagement()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(AddRoleViewModel addRoleViewModel)
        {

            if (!ModelState.IsValid)
                return View(addRoleViewModel);

            var role = new IdentityRole
            {
                Name = addRoleViewModel.RoleName
            };

            IdentityResult result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AdminController.RoleManagement));
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(addRoleViewModel);
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return RedirectToAction(nameof(AdminController.RoleManagement));
            
            var editRoleViewModel = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                Users = new List<string>()
            };

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                    editRoleViewModel.Users.Add(user.UserName);
            }

            return View(editRoleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel editRoleViewModel)
        {
            if (!ModelState.IsValid)
                return View(editRoleViewModel);

            var role = await _roleManager.FindByIdAsync(editRoleViewModel.Id);

            if (role != null)
            {
                role.Name = editRoleViewModel.RoleName;

                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                    return RedirectToAction(nameof(AdminController.RoleManagement));

                ModelState.AddModelError("", _stringLocalizer["Role not updated, something went wrong."]);

                return View(editRoleViewModel);
            }

            return RedirectToAction(nameof(AdminController.RoleManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction(nameof(AdminController.RoleManagement));
                ModelState.AddModelError("", _stringLocalizer["Something went wrong while deleting this role."]);
            }
            else
            {
                ModelState.AddModelError("", _stringLocalizer["This role can't be found."]);
            }
            return View(nameof(AdminController.RoleManagement), _roleManager.Roles);
        }
        #endregion

        #region Users in roles

        public async Task<IActionResult> AddUserToRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return RedirectToAction(nameof(AdminController.RoleManagement), _roleManager.Roles);

            var addUserToRoleViewModel = new UserRoleViewModel { RoleId = role.Id };

            foreach (var user in _userManager.Users)
            {
                if (!await _userManager.IsInRoleAsync(user, role.Name))
                {
                    addUserToRoleViewModel.Users.Add(user);
                }
            }

            return View(addUserToRoleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(UserRoleViewModel userRoleViewModel)
        {
            var user = await _userManager.FindByIdAsync(userRoleViewModel.UserId);
            var role = await _roleManager.FindByIdAsync(userRoleViewModel.RoleId);

            if (user == null)
            {
                return NotFound(_stringLocalizer["User not found."]);
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AdminController.RoleManagement), _roleManager.Roles);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(userRoleViewModel);
        }

        public async Task<IActionResult> DeleteUserFromRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return RedirectToAction(nameof(AdminController.RoleManagement), _roleManager.Roles);

            var addUserToRoleViewModel = new UserRoleViewModel { RoleId = role.Id };

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    addUserToRoleViewModel.Users.Add(user);
                }
            }

            return View(addUserToRoleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserFromRole(UserRoleViewModel userRoleViewModel)
        {
            var user = await _userManager.FindByIdAsync(userRoleViewModel.UserId);
            var role = await _roleManager.FindByIdAsync(userRoleViewModel.RoleId);

            if (user == null)
            {
                return NotFound(_stringLocalizer["User not found."]);
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AdminController.RoleManagement), _roleManager.Roles);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(userRoleViewModel);
        }
        #endregion

        #region Helpers

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}