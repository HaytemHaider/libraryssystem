using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Controllers;
using LibraryApi.Services.Interfaces;
using LibraryApi.Models;

namespace LibraryApi.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        #region GetUsers Tests

        [Fact]
        public async Task GetUsers_ReturnsOk_WithUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User 1" },
                new User { Id = Guid.NewGuid(), Name = "User 2" }
            };

            _mockUserService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Equal(users.Count, ((List<User>)returnedUsers).Count);
            // Valfritt: Kontrollera att användarna i listan är desamma
        }

        [Fact]
        public async Task GetUsers_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var users = new List<User>();

            _mockUserService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Empty(returnedUsers);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = "Test User" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
            Assert.Equal("Test User", returnedUser.Name);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Användaren hittades inte.", notFoundResult.Value);
        }

        #endregion

        #region AddUser Tests

        [Fact]
        public async Task AddUser_ReturnsCreated_WhenServiceReturnsSuccess()
        {
            var userId = Guid.NewGuid();
            // Arrange
            var user = new User { Id = userId, Name = "New User" };
            var addedUser = new User { Id = userId, Name = "New User" };

            _mockUserService.Setup(s => s.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new ServiceResult { Success = true, Data = addedUser });

            // Act
            var result = await _controller.AddUser(user);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            // Kontrollera att actionnamnet är korrekt
            Assert.Equal(nameof(UsersController.GetUser), createdAtActionResult.ActionName);

            // Kontrollera att route-värdena innehåller rätt id
            Assert.Equal(addedUser.Id, createdAtActionResult.RouteValues["id"]);

            var returnedUser = Assert.IsType<User>(createdAtActionResult.Value);
            Assert.Equal(addedUser.Name, returnedUser.Name);
            Assert.Equal(addedUser.Id, returnedUser.Id);
        }

        [Fact]
        public async Task AddUser_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var user = new User { Name = "New User" };
            var errorMessage = "Användaren kunde inte läggas till.";

            _mockUserService.Setup(s => s.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.AddUser(user);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedMessage = "Användaren har tagits bort.";

            _mockUserService.Setup(s => s.DeleteUserAsync(userId, false, null))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var errorMessage = "Användaren kunde inte tas bort.";

            _mockUserService.Setup(s => s.DeleteUserAsync(userId, false, null))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteUser_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var force = true;
            var repair = "return";
            var expectedMessage = "Användaren har tagits bort.";

            _mockUserService.Setup(s => s.DeleteUserAsync(userId, force, repair))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.DeleteUser(userId, force, repair);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);

            _mockUserService.Verify(s => s.DeleteUserAsync(userId, force, repair), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenServiceReturnsSuccess_WithRepairMode()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var force = true;
            var repair = "return"; // Eller "remove"
            var expectedMessage = "Användaren har tagits bort.";

            _mockUserService.Setup(s => s.DeleteUserAsync(userId, force, repair))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.DeleteUser(userId, force, repair);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);

            _mockUserService.Verify(s => s.DeleteUserAsync(userId, force, repair), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenServiceReturnsSuccess_WithRepairModeRemove()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var force = true;
            var repair = "remove";
            var expectedMessage = "Användaren har tagits bort.";

            _mockUserService.Setup(s => s.DeleteUserAsync(userId, force, repair))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.DeleteUser(userId, force, repair);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);

            _mockUserService.Verify(s => s.DeleteUserAsync(userId, force, repair), Times.Once);
        }

        [Theory]
        [InlineData("return")]
        [InlineData("remove")]
        public async Task DeleteUser_ReturnsOk_WhenServiceReturnsSuccess_WithRepairModes(string repair)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var force = true;
            var expectedMessage = "Användaren har tagits bort.";

            _mockUserService.Setup(s => s.DeleteUserAsync(userId, force, repair))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.DeleteUser(userId, force, repair);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);

            _mockUserService.Verify(s => s.DeleteUserAsync(userId, force, repair), Times.Once);
        }



        #endregion
    }
}
