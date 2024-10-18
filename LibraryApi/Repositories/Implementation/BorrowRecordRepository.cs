using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories.Implementation
{
    public class BorrowRecordRepository : IBorrowRecordRepository
    {
        private readonly LibraryContext _context;

        public BorrowRecordRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<BorrowRecord> GetActiveBorrowRecordAsync(Guid userId, Guid bookId)
        {
            return await _context.BorrowRecords
                .FirstOrDefaultAsync(br => br.UserId == userId && br.BookId == bookId && br.ReturnedAt == null);
        }

        public void AddBorrowRecord(BorrowRecord borrowRecord)
        {
            _context.BorrowRecords.Add(borrowRecord);
        }
    }


}
