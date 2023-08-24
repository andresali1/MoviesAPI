namespace MoviesAPI.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int Score { get; set; }
        public int MovieId { get; set; }
        public string AppUserId { get; set; }
        public string UserName { get; set; }
    }
}
