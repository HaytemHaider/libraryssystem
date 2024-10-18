using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Controllers;
using LibraryApi.Services.Interfaces;
using LibraryApi.Models;
using LibraryApi.Dto;
using LibraryApi.Services.Implementations;

namespace LibraryApi.Tests.Controllers
{
    public class LibraryControllerTests
    {
        private readonly Mock<ILibraryService> _mockLibraryService;
        private readonly LibraryController _controller;

        public LibraryControllerTests()
        {
            _mockLibraryService = new Mock<ILibraryService>();
            _controller = new LibraryController(_mockLibraryService.Object);
        }

        #region BorrowBook Tests

        [Fact]
        public async Task BorrowBook_ReturnsOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barcode = "1234567890123";
            var expectedMessage = "Boken har lånats ut.";

            _mockLibraryService.Setup(s => s.BorrowBookAsync(userId, barcode))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.BorrowBook(userId, barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);
        }

        [Fact]
        public async Task BorrowBook_ReturnsBadRequest_WhenUserIdIsNull()
        {
            // Arrange
            Guid? userId = null;
            var barcode = "1234567890123";

            // Act
            var result = await _controller.BorrowBook(userId, barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Query-parametrarna 'userId' och 'barcode' är obligatoriska.", badRequestResult.Value);
        }

        [Fact]
        public async Task BorrowBook_ReturnsBadRequest_WhenBarcodeIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            string barcode = null;

            // Act
            var result = await _controller.BorrowBook(userId, barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Query-parametrarna 'userId' och 'barcode' är obligatoriska.", badRequestResult.Value);
        }

        [Fact]
        public async Task BorrowBook_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barcode = "1234567890123";
            var errorMessage = "Användaren har redan lånat max antal böcker.";

            _mockLibraryService.Setup(s => s.BorrowBookAsync(userId, barcode))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.BorrowBook(userId, barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region ReturnBook Tests

        [Fact]
        public async Task ReturnBook_ReturnsOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barcode = "1234567890123";
            var expectedMessage = "Boken har lämnats tillbaka.";

            _mockLibraryService.Setup(s => s.ReturnBookAsync(userId, barcode))
                .ReturnsAsync(new ServiceResult { Success = true, Message = expectedMessage });

            // Act
            var result = await _controller.ReturnBook(userId, barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);
        }

        [Fact]
        public async Task ReturnBook_ReturnsBadRequest_WhenUserIdIsNull()
        {
            // Arrange
            Guid? userId = null;
            var barcode = "1234567890123";

            // Act
            var result = await _controller.ReturnBook(userId, barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Query-parametrarna 'userId' och 'barcode' är obligatoriska.", badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnBook_ReturnsBadRequest_WhenBarcodeIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            string barcode = null;

            // Act
            var result = await _controller.ReturnBook(userId, barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Query-parametrarna 'userId' och 'barcode' är obligatoriska.", badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnBook_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barcode = "1234567890123";
            var errorMessage = "Denna bok är inte utlånad till denna användare.";

            _mockLibraryService.Setup(s => s.ReturnBookAsync(userId, barcode))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.ReturnBook(userId, barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region GetUserBorrows Tests

        [Fact]
        public async Task GetUserBorrows_ReturnsOk_WhenServiceReturnsData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var borrowRecords = new List<BorrowRecordDto>
            {
                new BorrowRecordDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 1",
                    BorrowedAt = DateTime.UtcNow.AddDays(-10),
                    ReturnedAt = null
                },
                new BorrowRecordDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Book 2",
                    BorrowedAt = DateTime.UtcNow.AddDays(-5),
                    ReturnedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockLibraryService.Setup(s => s.GetUserBorrowRecordsAsync(userId))
                .ReturnsAsync(new ServiceResult { Success = true, Data = borrowRecords });

            // Act
            var result = await _controller.GetUserBorrows(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRecords = Assert.IsAssignableFrom<IEnumerable<BorrowRecordDto>>(okResult.Value);

            Assert.Equal(borrowRecords.Count, returnedRecords.Count());

            for (int i = 0; i < borrowRecords.Count; i++)
            {
                Assert.Equal(borrowRecords[i].Id, returnedRecords.ElementAt(i).Id);
                Assert.Equal(borrowRecords[i].Title, returnedRecords.ElementAt(i).Title);
                Assert.Equal(borrowRecords[i].BorrowedAt, returnedRecords.ElementAt(i).BorrowedAt);
                Assert.Equal(borrowRecords[i].ReturnedAt, returnedRecords.ElementAt(i).ReturnedAt);
            }
        }


        [Fact]
        public async Task GetUserBorrows_ReturnsNotFound_WhenServiceFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var errorMessage = "Användaren hittades inte.";

            _mockLibraryService.Setup(s => s.GetUserBorrowRecordsAsync(userId))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.GetUserBorrows(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(errorMessage, notFoundResult.Value);
        }

        #endregion

        #region GetBookBorrows Tests

        [Fact]
        public async Task GetBookBorrows_ReturnsOk_WhenServiceReturnsData()
        {
            // Arrange
            var barcode = "1234567890123";
            var borrowRecords = new List<BorrowRecordDto>
            {
                new BorrowRecordDto
                    {
                        Id = Guid.NewGuid(),
                        UserName = "Test User 1",
                        BorrowedAt = DateTime.UtcNow.AddDays(-7),
                        ReturnedAt = null
                    },
                new BorrowRecordDto
                    {
                        Id = Guid.NewGuid(),
                        UserName = "Test User 2",
                        BorrowedAt = DateTime.UtcNow.AddDays(-3),
                        ReturnedAt = DateTime.UtcNow.AddDays(-1)
                    }
            };

            _mockLibraryService.Setup(s => s.GetBookBorrowRecordsAsync(barcode))
                .ReturnsAsync(new ServiceResult { Success = true, Data = borrowRecords });

            // Act
            var result = await _controller.GetBookBorrows(barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRecords = Assert.IsAssignableFrom<IEnumerable<BorrowRecordDto>>(okResult.Value);

            Assert.Equal(borrowRecords.Count, returnedRecords.Count());

            for (int i = 0; i < borrowRecords.Count; i++)
            {
                Assert.Equal(borrowRecords[i].Id, returnedRecords.ElementAt(i).Id);
                Assert.Equal(borrowRecords[i].UserName, returnedRecords.ElementAt(i).UserName);
                Assert.Equal(borrowRecords[i].BorrowedAt, returnedRecords.ElementAt(i).BorrowedAt);
                Assert.Equal(borrowRecords[i].ReturnedAt, returnedRecords.ElementAt(i).ReturnedAt);
            }
        }


        [Fact]
        public async Task GetBookBorrows_ReturnsNotFound_WhenServiceFails()
        {
            // Arrange
            var barcode = "1234567890123";
            var errorMessage = "Boken hittades inte.";

            _mockLibraryService.Setup(s => s.GetBookBorrowRecordsAsync(barcode))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.GetBookBorrows(barcode);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(errorMessage, notFoundResult.Value);
        }


        #endregion
    }
}
