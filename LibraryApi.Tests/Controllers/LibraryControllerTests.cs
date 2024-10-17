using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Controllers;
using LibraryApi.Dto;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Models;
using LibraryApi.Data;

namespace LibraryApi.Tests.Controllers
{
    public class LibraryControllerTests
    {
        private readonly DbContextOptions<LibraryContext> _options;

        public LibraryControllerTests()
        {
            // Konfigurera in-memory databas för testning
            _options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task BorrowBook_UserHasMaxBooks_ReturnsBadRequest()
        {
            // Arrange
            using var context = new LibraryContext(_options);

            var userId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var user = new User { Id = userId, Name = "Test User" };
            var book = new Book
            {
                Id = bookId,
                Title = "Test Book",
                Barcode = "1234567890123",
                TotalCopies = 1,
                AvailableCopies = 1
            };

            // Lägg till användaren och boken i databasen
            context.Users.Add(user);
            context.Books.Add(book);
            context.SaveChanges();

            // Simulera att användaren redan har lånat 3 böcker
            for (int i = 0; i < 3; i++)
            {
                context.BorrowRecords.Add(new BorrowRecord
                {
                    UserId = userId,
                    BookId = Guid.NewGuid(),
                    BorrowedAt = DateTime.UtcNow
                });
            }
            context.SaveChanges();

            var controller = new LibraryController(context);

            var request = new BorrowRequest
            {
                UserId = userId,
                BookId = bookId
            };

            // Act
            var result = await controller.BorrowBook(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Användaren har redan lånat max antal böcker.", badRequestResult.Value);
        }
    }
}