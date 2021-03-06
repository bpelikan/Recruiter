﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Recruiter.Data;
using Recruiter.Models;
using Recruiter.Models.AdminViewModels;
using Recruiter.Services;
using Recruiter.Shared;

namespace Recruiter.Controllers
{
    [Authorize(Roles = RoleCollection.Administrator)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ICvStorageService _cvStorageService;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<AdminController> _stringLocalizer;
        private readonly IMapper _mapper;

        public AdminController(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext context,
            ICvStorageService cvStorageService,
            ILogger<AdminController> logger, 
            IStringLocalizer<AdminController> stringLocalizer, 
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _cvStorageService = cvStorageService;
            _logger = logger;
            _stringLocalizer = stringLocalizer;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            //return View();
            return RedirectToAction(nameof(AdminController.UserManagement));
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

            //var vm = _mapper.Map<ApplicationUser, UserDetailsViewModel>(user);
            var vm = new UserDetailsWithRolesViewModel()
            {
                User = _mapper.Map<ApplicationUser, UserDetailsViewModel>(user)
            };
            vm.User.CreatedAt = vm.User.CreatedAt.ToLocalTime();

            foreach (var role in _roleManager.Roles)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    vm.AddRole(role.Name, true);
                }
                else
                {
                    vm.AddRole(role.Name, false);
                }
            }
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
                CreatedAt = DateTime.UtcNow
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
                var usersApplications = _context.Applications.Where(x => x.UserId == user.Id);
                foreach (var application in usersApplications)
                {
                    var deleteResult = await _cvStorageService.DeleteCvAsync(application.CvFileName);
                    if (!deleteResult)
                    {
                        throw new Exception($"Something went wrong while deleting cv in Blob: {application.CvFileName}.");
                    }
                }

                var usersApplicationsViewHistory = _context.ApplicationsViewHistories.Where(x => x.UserId == user.Id);
                _context.ApplicationsViewHistories.RemoveRange(usersApplicationsViewHistory);
                await _context.SaveChangesAsync();

                var historyCount = _context.ApplicationsViewHistories.Where(x => x.UserId == user.Id).Count();
                if (historyCount != 0)
                {
                    throw new Exception($"Something went wrong while deleting users application view history. UserId: {user.Id}, HistoryCount: {historyCount}");
                }

                var applicatioApprovalResponsibilities = _context.ApplicationStagesRequirements.Where(x => x.DefaultResponsibleForApplicatioApprovalId == user.Id);
                foreach (var x in applicatioApprovalResponsibilities)
                    x.DefaultResponsibleForApplicatioApprovalId = null;

                var phoneCallResponsibilities = _context.ApplicationStagesRequirements.Where(x => x.DefaultResponsibleForPhoneCallId == user.Id);
                foreach (var x in phoneCallResponsibilities)
                    x.DefaultResponsibleForApplicatioApprovalId = null;

                var homeworkResponsibilities = _context.ApplicationStagesRequirements.Where(x => x.DefaultResponsibleForHomeworkId == user.Id);
                foreach (var x in homeworkResponsibilities)
                    x.DefaultResponsibleForApplicatioApprovalId = null;

                var interviewResponsibilities = _context.ApplicationStagesRequirements.Where(x => x.DefaultResponsibleForInterviewId == user.Id);
                foreach (var x in interviewResponsibilities)
                    x.DefaultResponsibleForApplicatioApprovalId = null;

                var interwievAppointments = _context.InterviewAppointments
                                                        .Include(x => x.Interview)
                                                        .Where(x => x.Interview.ResponsibleUserId == user.Id &&
                                                                    x.Interview.State != ApplicationStageState.Finished);
                if(interwievAppointments != null)
                    _context.InterviewAppointments.RemoveRange(interwievAppointments);

                var applicationStagesResponsibilities = _context.ApplicationStages.Where(x => x.ResponsibleUserId == user.Id);
                foreach (var x in applicationStagesResponsibilities)
                    x.ResponsibleUserId = null;

                await _context.SaveChangesAsync();


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
            };

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                    editRoleViewModel.AddUser(user);
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
                {
                    return RedirectToAction(nameof(AdminController.RoleManagement));
                }
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
        public async Task<IActionResult> AddUserToRole(string id, string roleName, string returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            var role = await _roleManager.FindByNameAsync(roleName);

            if (user == null)
            {
                return NotFound(_stringLocalizer["User not found."]);
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToLocal(returnUrl);
        }

        public async Task<IActionResult> DeleteUserFromRole(string id, string roleName, string returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            var role = await _roleManager.FindByNameAsync(roleName);

            if (user == null)
            {
                //throw new Exception($"User with id {id} not found. ({_userManager.GetUserId(HttpContext.User)})");
                return NotFound(_stringLocalizer["User not found."]);
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToLocal(returnUrl);
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