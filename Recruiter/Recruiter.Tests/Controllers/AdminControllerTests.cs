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

namespace Recruiter.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly AdminController controller;
        private readonly Mock<UserManager<ApplicationUser>> userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> roleManagerMock;
        private readonly Mock<ILogger<AdminController>> iLoggerMock;

        private readonly List<ApplicationUser> users = new List<ApplicationUser>
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
        private readonly AddUserViewModel viewModel = new AddUserViewModel()
        {
            Email = "test@test.com",
            FirstName = "FirstName",
            LastName = "LastName",
            Password = "Pass!23",
            PhoneNumber = "756845765"
        };

        public AdminControllerTests()
        {
            userManagerMock = MockHelpers.MockUserManager<ApplicationUser>();
            roleManagerMock = MockHelpers.MockRoleManager<IdentityRole>();
            iLoggerMock = MockHelpers.MockILogger<AdminController>();

            controller = new AdminController(userManagerMock.Object, roleManagerMock.Object, iLoggerMock.Object);
        }

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

        [Fact]
        public void UserManagement_None_ShouldReturnTypeViewResult()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.Users)
                .Returns(users.AsQueryable<ApplicationUser>());

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
                .Returns(users.AsQueryable<ApplicationUser>());

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
                .Returns(users.AsQueryable<ApplicationUser>());

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
                .Returns(users.AsQueryable<ApplicationUser>());

            // Act
            var result = controller.UserManagement();
            var test = result.As<ViewResult>().ViewData.Model;
            
            // Assert
            result.As<ViewResult>().ViewData.Model.Should().BeOfType<EnumerableQuery<ApplicationUser>>();
            result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(users);
            result.As<ViewResult>().ViewData.Model.As<EnumerableQuery<ApplicationUser>>().Count().Should().Be(users.Count);
        }

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

        [Fact]
        public void AddUser_ViewModelWithModelError_ShouldReturnViewModel()
        {
            // Arrange
            controller.ModelState.AddModelError("TestModelError","Model error from tests.");
            
            // Act
            var result = controller.AddUser(viewModel);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeOfType<AddUserViewModel>();
            result.Result.As<ViewResult>().ViewData.Model.Should().BeEquivalentTo(viewModel);
        }

        [Fact]
        public void AddUser_ValidViewModel_ShouldRedirectToUserManagementWithUsers()
        {
            // Arrange
            userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success));
            userManagerMock
               .Setup(m => m.Users)
               .Returns(users.AsQueryable<ApplicationUser>());

            // Act
            var result = controller.AddUser(viewModel);

            // Assert
            result.Should().BeOfType<Task<IActionResult>>();
            userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            result.Result.As<RedirectToActionResult>().ActionName.Should().BeEquivalentTo("UserManagement");
        }
    }   
}
