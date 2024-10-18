using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Repositories.Interfaces;
using LibraryApi.Services.Interfaces;

namespace LibraryApi.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowRecordRepository _borrowRecordRepository;
        private readonly LibraryContext _context;

        public BookService(
            IBookRepository bookRepository,
            IBorrowRecordRepository borrowRecordRepository,
            LibraryContext context)
        {
            _bookRepository = bookRepository;
            _borrowRecordRepository = borrowRecordRepository;
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllBooksAsync();
        }

        public async Task<Book> GetBookByIdAsync(Guid bookId)
        {
            return await _bookRepository.GetBookByIdAsync(bookId);
        }

        public async Task<Book> GetBookByBarcodeAsync(string barcode)
        {
            return await _bookRepository.GetBookByBarcodeAsync(barcode);
        }

        public async Task<ServiceResult> AddBookAsync(Book book)
        {
            if (await _bookRepository.BookExistsByBarcodeAsync(book.Barcode))
            {
                return new ServiceResult { Success = false, Message = "Streckkoden måste vara unik." };
            }

            book.Id = Guid.NewGuid();
            book.AvailableCopies = book.TotalCopies;

            _bookRepository.AddBook(book);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Boken har lagts till.", Data = book };
        }

        public async Task<ServiceResult> UpdateBookCountAsync(string barcode, int newTotalCopies)
        {
            var book = await _bookRepository.GetBookWithBorrowRecordsByBarcodeAsync(barcode);

            if (book == null)
            {
                return new ServiceResult { Success = false, Message = "Boken hittades inte." };
            }

            int currentlyBorrowed = book.BorrowRecords.Count(br => br.ReturnedAt == null);

            if (newTotalCopies < currentlyBorrowed)
            {
                int difference = currentlyBorrowed - newTotalCopies;
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Det totala antalet exemplar kan inte vara mindre än antalet utlånade exemplar ({currentlyBorrowed}). Det saknas {difference} exemplar för att täcka de utlånade böckerna."
                };
            }

            int differenceInTotalCopies = newTotalCopies - book.TotalCopies;
            book.TotalCopies = newTotalCopies;
            book.AvailableCopies += differenceInTotalCopies;

            if (book.AvailableCopies < 0)
            {
                int missingCopies = -book.AvailableCopies;
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Fel: Antalet tillgängliga exemplar blev negativt. Det saknas {missingCopies} exemplar för att matcha de utlånade böckerna."
                };
            }

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Antalet exemplar för boken har uppdaterats." };
        }

        public async Task<ServiceResult> DeleteBookAsync(string barcode, bool force)
        {
            var book = await _bookRepository.GetBookByBarcodeAsync(barcode);

            if (book == null)
            {
                return new ServiceResult { Success = false, Message = "Boken hittades inte." };
            }

            int currentlyBorrowed = book.BorrowRecords.Count(br => br.ReturnedAt == null);

            if (currentlyBorrowed > 0 && !force)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = $"Kan inte ta bort boken eftersom {currentlyBorrowed} exemplar är utlånade. Använd force-flaggan för att tvinga borttagning."
                };
            }

            _bookRepository.RemoveBook(book);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Boken har tagits bort." };
        }
    }

}
