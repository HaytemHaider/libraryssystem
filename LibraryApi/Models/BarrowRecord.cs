
using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class BorrowRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required]
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        [Required]
        public DateTime BorrowedAt { get; set; }

        public DateTime? ReturnedAt { get; set; }
    }
}