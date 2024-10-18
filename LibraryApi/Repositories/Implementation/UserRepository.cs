using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly LibraryContext _context;

        public UserRepository(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User> GetUserWithBorrowRecordsAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.BorrowRecords)
                    .ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public void RemoveUser(User user)
        {
            _context.Users.Remove(user);
        }
    }


}
