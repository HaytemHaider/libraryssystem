using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly LibraryContext _context;

        public UsersController(LibraryContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET /users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.BorrowRecords)
                    .ThenInclude(br => br.Book)
                .ToListAsync();
            return Ok(users);
        }

        // POST /users
        [HttpPost]
        public async Task<ActionResult<User>> AddUser(User user)
        {
            user.Id = Guid.NewGuid();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id, [FromQuery] bool force = false, [FromQuery] string repair = null)
        {
            var user = await _context.Users
                .Include(u => u.BorrowRecords)
                    .ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Användaren hittades inte.");
            }

            // Kolla om användaren har några låneposter
            var hasBorrowRecords = user.BorrowRecords.Any();

            if (hasBorrowRecords && !force)
            {
                return BadRequest("Användaren har låneposter. Använd force-flaggan för att tvinga borttagning.");
            }

            if (hasBorrowRecords && force)
            {
                if (string.IsNullOrEmpty(repair))
                {
                    return BadRequest("Repair-läget måste anges när force-flaggan är satt. Välj 'return' eller 'remove'.");
                }

                if (repair.Equals("return", StringComparison.OrdinalIgnoreCase))
                {
                    // Återlämna alla utlånade böcker
                    var borrowedBooks = user.BorrowRecords.Where(br => br.ReturnedAt == null).ToList();

                    foreach (var borrowRecord in borrowedBooks)
                    {
                        borrowRecord.ReturnedAt = DateTime.UtcNow;

                        // Uppdatera tillgängliga kopior av boken
                        var book = borrowRecord.Book;
                        if (book != null)
                        {
                            book.AvailableCopies += 1;
                        }
                    }
                }
                else if (repair.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    // Ta bort alla låneposter associerade med användaren
                    _context.BorrowRecords.RemoveRange(user.BorrowRecords);
                }
                else
                {
                    return BadRequest("Ogiltigt repair-läge. Välj 'return' eller 'remove'.");
                }
            }

            // Ta bort användaren
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok("Användaren har tagits bort.");
        }

    }
}