
using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}