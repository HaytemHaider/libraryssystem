using LibraryApi.Models;

namespace LibraryApi.Repositories.Interfaces
{
    public interface IBorrowRecordRepository
    {
        Task<BorrowRecord> GetActiveBorrowRecordAsync(Guid userId, Guid bookId);
        void AddBorrowRecord(BorrowRecord borrowRecord);
    }


}
