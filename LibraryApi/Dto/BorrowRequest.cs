
namespace LibraryApi.Dto
{
    public class BorrowRequest
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
    }
}
