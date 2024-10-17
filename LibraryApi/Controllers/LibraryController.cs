using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using LibraryApi.Data;


namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly LibraryContext _context;

        public LibraryController(LibraryContext context)
        {
            _context = context;
        }

        // POST /library/borrow
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook(int userId, int bookId)
        {
            var user = await _context.Users.Include(u => u.BorrowedBooks).FirstOrDefaultAsync(u => u.Id == userId);
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (user == null)
                return NotFound("Användaren hittades inte.");

            if (book == null)
                return NotFound("Boken hittades inte.");

            if (book.BorrowerId != null)
                return BadRequest("Boken är redan utlånad.");

            if (user.BorrowedBooks.Count >= 3)
                return BadRequest("Användaren har redan lånat max antal böcker.");

            book.BorrowerId = userId;
            user.BorrowedBooks.Add(book);

            await _context.SaveChangesAsync();

            return Ok("Boken har lånats ut.");
        }

        // POST /library/return
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook(int userId, int bookId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                return NotFound("Boken hittades inte.");

            if (book.BorrowerId != userId)
                return BadRequest("Denna bok är inte utlånad till denna användare.");
            book.BorrowerId = null;

            await _context.SaveChangesAsync();

            return Ok("Boken har lämnats tillbaka.");
        }
    }
}