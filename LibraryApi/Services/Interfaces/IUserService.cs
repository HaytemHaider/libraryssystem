using LibraryApi.Models;

namespace LibraryApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(Guid userId);
        Task<ServiceResult> AddUserAsync(User user);
        Task<ServiceResult> DeleteUserAsync(Guid userId, bool force, string repair);
    }


}
