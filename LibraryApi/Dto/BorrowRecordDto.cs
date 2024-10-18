
namespace LibraryApi.Dto
{
    public class BorrowRecordDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } // För GetUserBorrows
        public string UserName { get; set; } // För GetBookBorrows
        public DateTime BorrowedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}
