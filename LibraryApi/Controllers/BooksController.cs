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

        //TODO paginate
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

        [HttpPatch("update-count/{barcode}")]
        public async Task<IActionResult> UpdateBookCount(string barcode, [FromQuery] int? newTotalCopies)
        {
            if (newTotalCopies == null)
            {
                return BadRequest("Query-parametern 'newTotalCopies' är obligatorisk.");
            }

            if (newTotalCopies < 0)
            {
                return BadRequest("Ogiltig indata. 'newTotalCopies' måste vara 0 eller större.");
            }

            var book = await _context.Books
                .Include(b => b.BorrowRecords.Where(br => br.ReturnedAt == null))
                .FirstOrDefaultAsync(b => b.Barcode == barcode);

            if (book == null)
            {
                return NotFound("Bok med angiven streckkod hittades inte.");
            }

            // Antal exemplar som för närvarande är utlånade
            int currentlyBorrowed = book.BorrowRecords.Count();

            if (newTotalCopies < currentlyBorrowed)
            {
                int difference = currentlyBorrowed - newTotalCopies.Value;
                return BadRequest($"Det totala antalet exemplar kan inte vara mindre än antalet utlånade exemplar ({currentlyBorrowed}). " +
                    $"Det saknas {difference} exemplar för att täcka de utlånade böckerna.");
            }

            // Uppdatera total och tillgängliga exemplar
            int differenceInTotalCopies = newTotalCopies.Value - book.TotalCopies;
            book.TotalCopies = newTotalCopies.Value;
            book.AvailableCopies += differenceInTotalCopies;

            // Kontrollera om AvailableCopies är negativt efter uppdateringen
            if (book.AvailableCopies < 0)
            {
                int missingCopies = -book.AvailableCopies;
                return BadRequest($"Fel: Antalet tillgängliga exemplar blev negativt. Det saknas {missingCopies} exemplar för att matcha de utlånade böckerna.");
            }

            await _context.SaveChangesAsync();

            return Ok("Antalet exemplar för boken har uppdaterats.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id, [FromQuery] bool force = false)
        {
            var book = await _context.Books
                .Include(b => b.BorrowRecords.Where(br => br.ReturnedAt == null))
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound("Boken hittades inte.");
            }

            // Kolla om det finns utlånade exemplar
            int currentlyBorrowed = book.BorrowRecords.Count();

            if (currentlyBorrowed > 0 && !force)
            {
                return BadRequest($"Kan inte ta bort boken eftersom {currentlyBorrowed} exemplar är utlånade. Använd force-flaggan för att tvinga borttagning.");
            }

            // Ta bort boken (relaterade BorrowRecords tas bort via cascading delete)
            _context.Books.Remove(book);

            await _context.SaveChangesAsync();

            return Ok("Boken har tagits bort.");
        }

    }
}