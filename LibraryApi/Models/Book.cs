using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public required string Title { get; set; }

        [Required]
        [MaxLength(13)]
        public required string Barcode { get; set; } // Streckkod

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalCopies { get; set; } // Totalt antal kopior

        public int AvailableCopies { get; set; } // Tillgängliga kopior som kan lånas ut

        public ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}