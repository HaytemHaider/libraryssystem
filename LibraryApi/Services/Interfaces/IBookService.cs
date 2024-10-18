using LibraryApi.Models;

namespace LibraryApi.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(Guid bookId);
        Task<Book> GetBookByBarcodeAsync(string barcode);
        Task<ServiceResult> AddBookAsync(Book book);
        Task<ServiceResult> UpdateBookCountAsync(string barcode, int newTotalCopies);
        Task<ServiceResult> DeleteBookAsync(string barcode, bool force);
    }


}
