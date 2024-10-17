using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class Book
{
    [Key]
    public int Id { get; set; } // Unikt ID

    [Required]
    public required string Title { get; set; }

    public int? BorrowerId { get; set; } // Null om boken är tillgänglig
    public User? Borrower { get; set; } // Null om boken är tillgänglig
}
}