using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services.Interfaces;


namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly ILibraryService _libraryService;

        public LibraryController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        // POST /library/borrow
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromQuery] Guid? userId, [FromQuery] string barcode)
        {
            if (userId == null || string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest("Query-parametrarna 'userId' och 'barcode' är obligatoriska.");
            }

            var result = await _libraryService.BorrowBookAsync(userId.Value, barcode);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // POST /library/return
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromQuery] Guid? userId, [FromQuery] string barcode)
        {
            if (userId == null || string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest("Query-parametrarna 'userId' och 'barcode' är obligatoriska.");
            }

            var result = await _libraryService.ReturnBookAsync(userId.Value, barcode);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // GET /library/user-borrows/{userId}
        [HttpGet("user-borrows/{userId}")]
        public async Task<IActionResult> GetUserBorrows(Guid userId)
        {
            var result = await _libraryService.GetUserBorrowRecordsAsync(userId);

            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }

        // GET /library/book-borrows/{barcode}
        [HttpGet("book-borrows/{barcode}")]
        public async Task<IActionResult> GetBookBorrows(string barcode)
        {
            var result = await _libraryService.GetBookBorrowRecordsAsync(barcode);

            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }
    }
}