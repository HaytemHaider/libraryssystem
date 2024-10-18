
namespace LibraryApi.Dto
{
    public class BorrowRecordDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } // F�r GetUserBorrows
        public string UserName { get; set; } // F�r GetBookBorrows
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}
