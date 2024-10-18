using Microsoft.AspNetCore.Mvc;
using LibraryApi.Models;
using LibraryApi.Services.Interfaces;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET /books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        // POST /books
        [HttpPost]
        public async Task<ActionResult> AddBook([FromBody] Book book)
        {
            var result = await _bookService.AddBookAsync(book);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
        }

        // PATCH /books/update-count/{barcode}
        [HttpPatch("update-count/{barcode}")]
        public async Task<IActionResult> UpdateBookCount(string barcode, [FromQuery] int? newTotalCopies)
        {
            if (newTotalCopies == null)
            {
                return BadRequest("Query-parametern 'newTotalCopies' är obligatorisk.");
            }

            var result = await _bookService.UpdateBookCountAsync(barcode, newTotalCopies.Value);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // DELETE /books/{barcode}
        [HttpDelete("{barcode}")]
        public async Task<IActionResult> DeleteBook(string barcode, [FromQuery] bool force = false)
        {
            var result = await _bookService.DeleteBookAsync(barcode, force);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }

}