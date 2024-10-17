using LibraryApi.Models;

namespace LibraryApi.Data
{
    public class LibraryRepository
    {
        public List<Book> Books { get; set; } = new List<Book>();
        public List<User> Users { get; set; } = new List<User>();

        // Singleton
        private static LibraryRepository _instance;

        public static LibraryRepository Instance => _instance ?? (_instance = new LibraryRepository());
    }
}
