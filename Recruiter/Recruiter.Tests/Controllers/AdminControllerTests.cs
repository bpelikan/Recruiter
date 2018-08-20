using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recruiter.Controllers;
using Recruiter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Recruiter.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Recruiter.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly AdminController controller;
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
        private readonly Mock<ILogger<AdminController>> iLoggerMock;

        private static readonly ApplicationUser applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            FirstName = "Kamil",
            LastName = "Dumny",
            PhoneNumber = "821654987"
        };
        private static readonly List<ApplicationUser> applicationUsers = new List<ApplicationUser>
                                        {
                                            new ApplicationUser {
                                                Id = Guid.NewGuid().ToString(),
                                                FirstName = "Kamil",
                                                LastName = "Dumny",
                                                PhoneNumber = "321654987",
                                                 },
                                            new ApplicationUser {
                                                Id = Guid.NewGuid().ToString(),
                                                FirstName = "Jan",
                                                LastName = "Kowalski",
                                                PhoneNumber = "123456789",
                                                 },
                                            new ApplicationUser {
                                                Id = Guid.NewGuid().ToString(),
                                                FirstName = "Adam",
                                                LastName = "Miskiewisz",
                                                PhoneNumber = "789678567",
                                                },
                                            new ApplicationUser {
                                                Id = Guid.NewGuid().ToString(),
                                                FirstName = "Dorota",
                                                LastName = "Polak",
                                                PhoneNumber = "345678987",
                                                 },
                                            new ApplicationUser {
                                                Id = Guid.NewGuid().ToString(),
                                                FirstName = "Adam",
                                                LastName = "Romilak",
                                                PhoneNumber = "821639587",
                                                 }
                                        };
        private static readonly AddUserViewModel addUserViewModel = new AddUserViewModel
        {
            Email = "test@test.com",
            FirstName = "FirstName",
            LastName = "LastName",
            Password = "Pass!23",
            PhoneNumber = "756845765"
        };
        private static readonly EditUserViewModel editUserViewModelOld = new EditUserViewModel()
        {
            Id = applicationUser.Id,
            Email = applicationUser.Email,
            FirstName = applicationUser.FirstName,
            LastName = applicationUser.LastName,
            PhoneNumber = applicationUser.PhoneNumber,
        };
        private static readonly EditUserViewModel editUserViewModelNew = new EditUserViewModel()
        {
            Id = applicationUser.Id,
            Email = "test2@test.com",
            FirstName = "Adam",
            LastName = "Nowak",
            PhoneNumber = "485738456",
        };
        private static readonly IdentityError identityError = new IdentityError
        {
            Code = "",
            Description = "ErrorDescription"
        };


        public AdminControllerTests()
        {
            userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            iLoggerMock = MockHelpers.MockILogger<AdminController>();

            controller = new AdminController(userManagerMock.Object, roleManagerMock.Object, iLoggerMock.Object);
            //controller.ViewData.ModelState.Clear();
        }

        #region Index() Tests  
        [Fact]
        public void Index_None_ShouldReturnTypeViewResult()
        {
            // Arrange

            // Act
            var result = controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Index_None_ShouldInvokeViewWithEmptyViewNameParam()
        {
            // Arrange

            // Act
            var result = controller.Index();

            // Assert
            result.As<ViewResult>().ViewName.Should().BeNull();
        }

        [Fact]
        public void Index_None_ShouldNotReturnAnyData()
        {
            // Arrange

            // Act
            var result = controller.Index();

            // Assert
            result.As<ViewResult>().ViewData.Model.Should().BeNull();
        }
        #endregion  

        #region UserManagement() Tests  
        [Fact]
        public void UserManagement_None_ShouldReturnTypeViewResult()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.Users)
                .Returns(applicationUsers.AsQueryable<ApplicationUser>());

            // Act
            var result = controller.UserManagement();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void UserManagement_None_ShouldInvokeViewWithEmptyViewNameParam()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.Users)
                .Returns(applicationUsers.AsQueryable<ApplicationUser>());

            // Act
            var result = controller.UserManagement();

            // Assert
            result.As<ViewResult>().ViewName.Should().BeNull();
        }

        [Fact]
        public void UserManagement_None_ShouldInvokeUserManagerOnce()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.Users)
                .Returns(applicationUsers.AsQueryable<ApplicationUser>());

            // Act
            var result = controller.UserManagement();

            // Assert
            userManagerMock.Verify(m => m.Users, Times.Once());
        }

        [Fact]
        public void UserManagement_None_ShouldReturnUsers()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.Users)
                .Returns(applicationUsers.AsQueryable<ApplicationUser>());

            // Act
            var result = controller.UserManagement();
            var test = result.As<ViewResult>().ViewData.Model;
            
            // Assert
            result.As<ViewResult>().ViewData.Model.Should().BeOfType<EnumerableQuery<ApplicationUser>>();
            result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(applicationUsers);
            result.As<ViewResult>().ViewData.Model.As<EnumerableQuery<ApplicationUser>>().Count().Should().Be(applicationUsers.Count);
        }
        #endregion

        #region AddUser() Tests 
        [Fact]
        public void AddUser_None_ShouldReturnTypeViewResult()
        {
            // Arrange

            // Act
            var result = controller.AddUser();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void AddUser_None_ShouldInvokeViewWithEmptyViewNameParam()
        {

            // Act
            var result = controller.AddUser();

            // Assert
            result.As<ViewResult>().ViewName.Should().BeNull();
        }

        [Fact]
        public void AddUser_None_ShouldNotReturnAnyData()
        {
            // Arrange

            // Act
            var result = controller.AddUser();

            // Assert
            result.As<ViewResult>().ViewData.Model.Should().BeNull();
        }
        #endregion

        #region AddUser(AddUserViewModel addUserViewModel) Tests 
        [Fact]
        public void AddUser_ViewModelWithModelError_ShouldReturnViewModel()
        {
            // Arrange
            controller.ModelState.AddModelError("TestModelError","Model error from tests.");

            // Act
            var result = controller.AddUser(addUserViewModel);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeOfType<AddUserViewModel>();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(addUserViewModel);
        }

        [Fact]
        public void AddUser_ValidViewModel_ShouldRedirectToUserManagement()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success));

            var user = new ApplicationUser()
            {
                UserName = addUserViewModel.Email.Normalize().ToUpper(),
                Email = addUserViewModel.Email,
                FirstName = addUserViewModel.FirstName,
                LastName = addUserViewModel.LastName,
                PhoneNumber = addUserViewModel.PhoneNumber
            };

            // Act
            var result = controller.AddUser(addUserViewModel);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(x => x.UserName == user.UserName &&
                                                                                x.Email == user.Email &&
                                                                                x.FirstName == user.FirstName &&
                                                                                x.LastName == user.LastName &&
                                                                                x.PhoneNumber == user.PhoneNumber), addUserViewModel.Password), Times.Once);
            result.Result.As<RedirectToActionResult>().ActionName.Should().BeEquivalentTo("UserManagement");
        }

        [Fact]
        public void AddUser_ViewModel_CreateAsyncWithIdentityErrorShouldReturnViewModelWithModelError()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Failed(identityError)));

            var user = new ApplicationUser()
            {
                UserName = addUserViewModel.Email.Normalize().ToUpper(),
                Email = addUserViewModel.Email,
                FirstName = addUserViewModel.FirstName,
                LastName = addUserViewModel.LastName,
                PhoneNumber = addUserViewModel.PhoneNumber
            };

            // Act
            var result = controller.AddUser(addUserViewModel);
            
            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(x => x.UserName == user.UserName &&
                                                                                x.Email == user.Email &&
                                                                                x.FirstName == user.FirstName &&
                                                                                x.LastName == user.LastName &&
                                                                                x.PhoneNumber == user.PhoneNumber), addUserViewModel.Password), Times.Once);
            result.Result.As<ViewResult>().ViewName.Should().BeNull();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(addUserViewModel);
            controller.ViewData.ModelState.Root.Errors[0].ErrorMessage.Should().BeEquivalentTo(identityError.Description);
        }
        #endregion

        #region EditUser(string id) Tests 
        [Fact]
        public void EditUser_InvalidId_ShouldRedirectToUserManagement()
        {
            // Arrange
            userManagerMock
               .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
               .Returns(Task.FromResult<ApplicationUser>(null));

            var id = Guid.NewGuid().ToString();

            // Act
            var result = controller.EditUser(id);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.FindByIdAsync(id), Times.Once);
            result.Result.As<RedirectToActionResult>().ActionName.Should().BeEquivalentTo("UserManagement");
        }

        [Fact]
        public void EditUser_ValidId_ShouldReturnViewModel()
        {
            // Arrange
            userManagerMock
               .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
               .Returns(Task.FromResult<ApplicationUser>(applicationUser));

            var id = Guid.NewGuid().ToString();

            // Act
            var result = controller.EditUser(id);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.FindByIdAsync(id), Times.Once);
            result.Result.As<ViewResult>().ViewName.Should().BeNull();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(editUserViewModelOld);
        }
        #endregion

        #region EditUser(EditUserViewModel editUserViewModel) Tests 
        [Fact]
        public void EditUser_EditUserViewModelUserNotExist_ShouldRedirectToUserManagement()
        {
            // Arrange
            userManagerMock
               .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
               .Returns(Task.FromResult<ApplicationUser>(null));

            // Act
            var result = controller.EditUser(editUserViewModelNew);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.FindByIdAsync(editUserViewModelNew.Id), Times.Once);
            result.Result.As<RedirectToActionResult>().ActionName.Should().BeEquivalentTo("UserManagement");
        }

        [Fact]
        public void EditUser_ValidEditUserViewModel_ShouldRedirectToUserManagement()
        {
            // Arrange
            userManagerMock
               .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
               .Returns(Task.FromResult<ApplicationUser>(applicationUser));
            userManagerMock
               .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
               .Returns(Task.FromResult(IdentityResult.Success));

            ApplicationUser user = applicationUser;
            user.UserName = editUserViewModelNew.Email.Normalize().ToUpper();
            user.Email = editUserViewModelNew.Email;
            user.FirstName = editUserViewModelNew.FirstName;
            user.LastName = editUserViewModelNew.LastName;
            user.PhoneNumber = editUserViewModelNew.PhoneNumber;
            
            // Act
            var result = controller.EditUser(editUserViewModelNew);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.FindByIdAsync(editUserViewModelNew.Id), Times.Once);
            userManagerMock.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(x => x.Id == user.Id &&
                                                                                x.UserName == user.UserName &&
                                                                                x.Email == user.Email &&
                                                                                x.FirstName == user.FirstName &&
                                                                                x.LastName == user.LastName &&
                                                                                x.PhoneNumber == user.PhoneNumber)), Times.Once);
            result.Result.As<RedirectToActionResult>().ActionName.Should().BeEquivalentTo("UserManagement");
        }

        [Fact]
        public void EditUser_ValidEditUserViewModel_UpdateAsyncWithIdentityErrorShouldReturnViewModelWithModelError()
        {
            // Arrange
            userManagerMock
               .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
               .Returns(Task.FromResult<ApplicationUser>(applicationUser));
            userManagerMock
               .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
               .Returns(Task.FromResult(IdentityResult.Failed()));

            ApplicationUser user = applicationUser;
            user.UserName = editUserViewModelNew.Email.Normalize().ToUpper();
            user.Email = editUserViewModelNew.Email;
            user.FirstName = editUserViewModelNew.FirstName;
            user.LastName = editUserViewModelNew.LastName;
            user.PhoneNumber = editUserViewModelNew.PhoneNumber;

            // Act
            var result = controller.EditUser(editUserViewModelOld);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.FindByIdAsync(editUserViewModelNew.Id), Times.Once);
            userManagerMock.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(x => x.Id == user.Id &&
                                                                                x.UserName == user.UserName &&
                                                                                x.Email == user.Email &&
                                                                                x.FirstName == user.FirstName &&
                                                                                x.LastName == user.LastName &&
                                                                                x.PhoneNumber == user.PhoneNumber)), Times.Once);
            
            result.Result.As<ViewResult>().ViewName.Should().BeNull();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(user);
            controller.ViewData.ModelState.Root.Errors[0].ErrorMessage.Should().BeEquivalentTo("User not updated, something went wrong.");
        }
        #endregion
    }
}
