using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Data;
using LibraryApi.Models;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET /books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.BorrowRecords)
                    .ThenInclude(br => br.User)
                .ToListAsync();
            return Ok(books);
        }

        // POST /books
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            if (_context.Books.Any(b => b.Barcode == book.Barcode))
            {
                return BadRequest("Streckkoden måste vara unik.");
            }

            book.Id = Guid.NewGuid();
            book.AvailableCopies = book.TotalCopies;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
        }
    }
}