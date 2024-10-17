
using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class User
{
    [Key]
    public int Id { get; set; } // Unikt ID

    [Required]
    public string Name { get; set; }

    public ICollection<Book> BorrowedBooks { get; set; } = new List<Book>();
}
}