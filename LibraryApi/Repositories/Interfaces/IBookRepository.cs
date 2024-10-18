using LibraryApi.Models;

namespace LibraryApi.Repositories.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(Guid bookId);
        Task<Book> GetBookByBarcodeAsync(string barcode);
        Task<Book> GetBookWithBorrowRecordsAsync(Guid bookId);
        Task<Book> GetBookWithBorrowRecordsByBarcodeAsync(string barcode);
        Task<bool> BookExistsByBarcodeAsync(string barcode);
        void AddBook(Book book);
        void RemoveBook(Book book);
    }

}
