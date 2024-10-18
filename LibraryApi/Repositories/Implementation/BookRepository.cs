using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories.Implementation
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(Guid bookId)
        {
            return await _context.Books.FindAsync(bookId);
        }

        public async Task<Book> GetBookByBarcodeAsync(string barcode)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.Barcode == barcode);
        }

        public async Task<Book> GetBookWithBorrowRecordsAsync(Guid bookId)
        {
            return await _context.Books
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.Id == bookId);
        }

        public async Task<Book> GetBookWithBorrowRecordsByBarcodeAsync(string barcode)
        {
            return await _context.Books
                .Include(b => b.BorrowRecords)
                    .ThenInclude(br => br.User)
                .FirstOrDefaultAsync(b => b.Barcode == barcode);
        }

        public async Task<bool> BookExistsByBarcodeAsync(string barcode)
        {
            return await _context.Books.AnyAsync(b => b.Barcode == barcode);
        }

        public void AddBook(Book book)
        {
            _context.Books.Add(book);
        }

        public void RemoveBook(Book book)
        {
            _context.Books.Remove(book);
        }
    }


}
