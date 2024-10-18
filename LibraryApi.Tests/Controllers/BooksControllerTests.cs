using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Controllers;
using LibraryApi.Services.Interfaces;
using LibraryApi.Models;

namespace LibraryApi.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BooksController(_mockBookService.Object);
        }

        #region GetBooks Tests

        [Fact]
        public async Task GetBooks_ReturnsOk_WithBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", Barcode = "1234567890123" },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Barcode = "9876543210987" }
            };

            _mockBookService.Setup(s => s.GetAllBooksAsync())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
            Assert.Equal(books.Count, ((List<Book>)returnedBooks).Count);
        }

        [Fact]
        public async Task GetBooks_ReturnsOk_WithEmptyList()
        {
            // Arrange
            var books = new List<Book>();

            _mockBookService.Setup(s => s.GetAllBooksAsync())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
            Assert.Empty(returnedBooks);
        }

        #endregion

        #region AddBook Tests

        [Fact]
        public async Task AddBook_ReturnsCreated_WhenServiceReturnsSuccess()
        {
            var boodId = Guid.NewGuid();
            // Arrange
            var book = new Book { Id = boodId, Title = "New Book", Barcode = "1234567890123", TotalCopies = 5 };
            var addedBook = new Book { Id = boodId, Title = "New Book", Barcode = "1234567890123", TotalCopies = 5, AvailableCopies = 5};

            _mockBookService.Setup(s => s.AddBookAsync(It.IsAny<Book>()))
                .ReturnsAsync(new ServiceResult { Success = true, Data = addedBook });

            // Act
            var result = await _controller.AddBook(book);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedBook = Assert.IsType<Book>(createdAtActionResult.Value);
            Assert.Equal(addedBook.Id, returnedBook.Id);
            Assert.Equal(addedBook.Title, returnedBook.Title);
            Assert.Equal(addedBook.Barcode, returnedBook.Barcode);
        }

        [Fact]
        public async Task AddBook_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var book = new Book { Title = "New Book", Barcode = "1234567890123", TotalCopies = 5 };
            var errorMessage = "Barcode must be unique.";

            _mockBookService.Setup(s => s.AddBookAsync(It.IsAny<Book>()))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.AddBook(book);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region UpdateBookCount Tests

        [Fact]
        public async Task UpdateBookCount_ReturnsOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var barcode = "1234567890123";
            var newTotalCopies = 10;
            var successMessage = "Book copy count updated successfully.";

            _mockBookService.Setup(s => s.UpdateBookCountAsync(barcode, newTotalCopies))
                .ReturnsAsync(new ServiceResult { Success = true, Message = successMessage });

            // Act
            var result = await _controller.UpdateBookCount(barcode, newTotalCopies);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(successMessage, okResult.Value);
        }

        [Fact]
        public async Task UpdateBookCount_ReturnsBadRequest_WhenNewTotalCopiesIsNull()
        {
            // Arrange
            var barcode = "1234567890123";

            // Act
            var result = await _controller.UpdateBookCount(barcode, null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Query-parametern 'newTotalCopies' är obligatorisk.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateBookCount_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var barcode = "1234567890123";
            var newTotalCopies = 2;
            var errorMessage = "Cannot set total copies less than borrowed copies.";

            _mockBookService.Setup(s => s.UpdateBookCountAsync(barcode, newTotalCopies))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.UpdateBookCount(barcode, newTotalCopies);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region DeleteBook Tests

        [Fact]
        public async Task DeleteBook_ReturnsOk_WhenServiceReturnsSuccess()
        {
            // Arrange
            var barcode = "123456789";
            var successMessage = "Book has been deleted.";

            _mockBookService.Setup(s => s.DeleteBookAsync(barcode, false))
                .ReturnsAsync(new ServiceResult { Success = true, Message = successMessage });

            // Act
            var result = await _controller.DeleteBook(barcode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(successMessage, okResult.Value);
        }

        [Fact]
        public async Task DeleteBook_ReturnsBadRequest_WhenServiceFails()
        {
            // Arrange
            var barcode = "123456789";
            var errorMessage = "Cannot delete book; copies are currently borrowed.";

            _mockBookService.Setup(s => s.DeleteBookAsync(barcode, false))
                .ReturnsAsync(new ServiceResult { Success = false, Message = errorMessage });

            // Act
            var result = await _controller.DeleteBook(barcode);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteBook_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var barcode = "123456789";
            var force = true;
            var successMessage = "Book has been deleted.";

            _mockBookService.Setup(s => s.DeleteBookAsync(barcode, force))
                .ReturnsAsync(new ServiceResult { Success = true, Message = successMessage });

            // Act
            var result = await _controller.DeleteBook(barcode, force);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(successMessage, okResult.Value);

            _mockBookService.Verify(s => s.DeleteBookAsync(barcode, force), Times.Once);
        }

        #endregion
    }
}
