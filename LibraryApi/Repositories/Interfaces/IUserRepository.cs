using LibraryApi.Models;

namespace LibraryApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid userId);
        Task<User> GetUserWithBorrowRecordsAsync(Guid userId);
        void AddUser(User user);
        void RemoveUser(User user);
    }

}
