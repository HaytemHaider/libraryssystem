using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Repositories.Interfaces;
using LibraryApi.Services.Interfaces;

namespace LibraryApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly IBookRepository _bookRepository;
        private readonly LibraryContext _context;

        public UserService(
            IUserRepository userRepository,
            IBorrowRecordRepository borrowRecordRepository,
            IBookRepository bookRepository,
            LibraryContext context)
        {
            _userRepository = userRepository;
            _borrowRecordRepository = borrowRecordRepository;
            _bookRepository = bookRepository;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<ServiceResult> AddUserAsync(User user)
        {
            user.Id = Guid.NewGuid();
            _userRepository.AddUser(user);
            await _context.SaveChangesAsync();
            return new ServiceResult { Success = true, Message = "Användaren har lagts till.", Data = user };
        }

        public async Task<ServiceResult> DeleteUserAsync(Guid userId, bool force, string repair)
        {
            var user = await _userRepository.GetUserWithBorrowRecordsAsync(userId);

            if (user == null)
            {
                return new ServiceResult { Success = false, Message = "Användaren hittades inte." };
            }

            var hasBorrowRecords = user.BorrowRecords.Any();

            if (hasBorrowRecords && !force)
            {
                return new ServiceResult { Success = false, Message = "Användaren har låneposter. Använd force-flaggan för att tvinga borttagning." };
            }

            if (hasBorrowRecords && force)
            {
                if (string.IsNullOrEmpty(repair))
                {
                    return new ServiceResult { Success = false, Message = "Repair-läget måste anges när force-flaggan är satt. Välj 'return' eller 'remove'." };
                }

                if (repair.Equals("return", StringComparison.OrdinalIgnoreCase))
                {
                    var borrowedBooks = user.BorrowRecords.Where(br => br.ReturnedAt == null).ToList();

                    foreach (var borrowRecord in borrowedBooks)
                    {
                        borrowRecord.ReturnedAt = DateTime.UtcNow;

                        var book = await _bookRepository.GetBookByIdAsync(borrowRecord.BookId);
                        if (book != null)
                        {
                            book.AvailableCopies += 1;
                        }
                    }
                }
                else if (repair.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    // Cascading deletes tar hand om detta
                }
                else
                {
                    return new ServiceResult { Success = false, Message = "Ogiltigt repair-läge. Välj 'return' eller 'remove'." };
                }
            }

            _userRepository.RemoveUser(user);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Användaren har tagits bort." };
        }
    }

}
