using LibraryApi.Models;

namespace LibraryApi.Services.Interfaces
{
    public interface ILibraryService
    {
        Task<ServiceResult> BorrowBookAsync(Guid userId, string barcode);
        Task<ServiceResult> ReturnBookAsync(Guid userId, string barcode);
        Task<ServiceResult> GetUserBorrowRecordsAsync(Guid userId);
        Task<ServiceResult> GetBookBorrowRecordsAsync(string barcode);
    }



}
