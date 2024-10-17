using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using LibraryApi.Data;
using LibraryApi.Dto;


namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly LibraryContext _context;

        public LibraryController(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // POST /library/borrow
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromQuery] Guid? userId, [FromQuery] Guid? bookId)
        {
            if (userId == null || bookId == null)
            {
                return BadRequest("Query-parametrarna 'userId' och 'bookId' är obligatoriska.");
            }

            var user = await _context.Users
                .Include(u => u.BorrowRecords)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            var book = await _context.Books
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.Id == bookId.Value);

            if (user == null)
                return NotFound("Användaren hittades inte.");

            if (book == null)
                return NotFound("Boken hittades inte.");

            var activeBorrowings = user.BorrowRecords.Count(br => br.ReturnedAt == null);

            if (activeBorrowings >= 3)
                return BadRequest("Användaren har redan lånat max antal böcker.");

            if (book.AvailableCopies <= 0)
                return BadRequest("Inga tillgängliga kopior av denna bok.");

            // Skapa ett nytt lånepost
            var borrowRecord = new BorrowRecord
            {
                UserId = userId.Value,
                BookId = bookId.Value,
                BorrowedAt = DateTime.UtcNow
            };

            book.AvailableCopies -= 1;

            _context.BorrowRecords.Add(borrowRecord);
            await _context.SaveChangesAsync();

            return Ok("Boken har lånats ut.");
        }


        // POST /library/return
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromQuery] Guid? userId, [FromQuery] Guid? bookId)
        {
            if (userId == null || bookId == null)
            {
                return BadRequest("Query-parametrarna 'userId' och 'bookId' är obligatoriska.");
            }

            var borrowRecord = await _context.BorrowRecords
                .FirstOrDefaultAsync(br => br.UserId == userId.Value && br.BookId == bookId.Value && br.ReturnedAt == null);

            if (borrowRecord == null)
                return BadRequest("Denna bok är inte utlånad till denna användare.");

            borrowRecord.ReturnedAt = DateTime.UtcNow;

            // Uppdatera tillgängliga kopior
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId.Value);
            if (book != null)
            {
                book.AvailableCopies += 1;
            }

            await _context.SaveChangesAsync();

            return Ok("Boken har lämnats tillbaka.");
        }

    }
}