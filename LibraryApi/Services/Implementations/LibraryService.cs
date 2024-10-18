using LibraryApi.Data;
using LibraryApi.Dto;
using LibraryApi.Models;
using LibraryApi.Repositories.Interfaces;
using LibraryApi.Services.Interfaces;

namespace LibraryApi.Services.Implementations
{
    public class LibraryService : ILibraryService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly LibraryContext _context;

        public LibraryService(
            IUserRepository userRepository,
            IBookRepository bookRepository,
            IBorrowRecordRepository borrowRecordRepository,
            LibraryContext context)
        {
            _userRepository = userRepository;
            _bookRepository = bookRepository;
            _borrowRecordRepository = borrowRecordRepository;
            _context = context;
        }

        public async Task<ServiceResult> BorrowBookAsync(Guid userId, string barcode)
        {
            var user = await _userRepository.GetUserWithBorrowRecordsAsync(userId);
            var book = await _bookRepository.GetBookByBarcodeAsync(barcode);

            if (user == null)
                return new ServiceResult { Success = false, Message = "Användaren hittades inte." };

            if (book == null)
                return new ServiceResult { Success = false, Message = "Boken hittades inte." };

            var activeBorrowings = user.BorrowRecords.Count(br => br.ReturnedAt == null);

            if (activeBorrowings >= 3)
                return new ServiceResult { Success = false, Message = "Användaren har redan lånat max antal böcker." };

            if (book.AvailableCopies <= 0)
                return new ServiceResult { Success = false, Message = "Inga tillgängliga kopior av denna bok." };

            var borrowRecord = new BorrowRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = user,
                BookId = book.Id,
                Book = book,
                BorrowedAt = DateTime.UtcNow
            };

            book.AvailableCopies -= 1;

            _borrowRecordRepository.AddBorrowRecord(borrowRecord);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Boken har lånats ut." };
        }

        public async Task<ServiceResult> ReturnBookAsync(Guid userId, string barcode)
        {
            var book = await _bookRepository.GetBookByBarcodeAsync(barcode);

            if (book == null)
                return new ServiceResult { Success = false, Message = "Boken hittades inte." };

            var borrowRecord = await _borrowRecordRepository.GetActiveBorrowRecordAsync(userId, book.Id);

            if (borrowRecord == null)
                return new ServiceResult { Success = false, Message = "Denna bok är inte utlånad till denna användare." };

            borrowRecord.ReturnedAt = DateTime.UtcNow;
            book.AvailableCopies += 1;

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Boken har lämnats tillbaka." };
        }

        public async Task<ServiceResult> GetUserBorrowRecordsAsync(Guid userId)
        {
            var user = await _userRepository.GetUserWithBorrowRecordsAsync(userId);

            if (user == null)
                return new ServiceResult { Success = false, Message = "Användaren hittades inte." };

            var borrowRecords = user.BorrowRecords.Select(br => new BorrowRecordDto
            {
                Id = br.Id,
                Title = br.Book.Title,
                UserName = br.User.Name,
                BorrowedAt = br.BorrowedAt,
                ReturnedAt = br.ReturnedAt
            });

            return new ServiceResult { Success = true, Data = borrowRecords };
        }

        public async Task<ServiceResult> GetBookBorrowRecordsAsync(string barcode)
        {
            var book = await _bookRepository.GetBookWithBorrowRecordsByBarcodeAsync(barcode);

            if (book == null)
                return new ServiceResult { Success = false, Message = "Boken hittades inte." };

            var borrowRecords = book.BorrowRecords.Select(br => new
            {
                br.Id,
                UserName = br.User.Name,
                br.BorrowedAt,
                br.ReturnedAt
            });

            return new ServiceResult { Success = true, Data = borrowRecords };
        }
    }



}
