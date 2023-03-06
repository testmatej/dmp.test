namespace rssreader.Models
{
    public class Article
    {
        public int Id { get; set; }
        public int FeedId { get; set; }
        public string? Title { get; set; }
        public string? URL { get; set; }
        public string? Description { get; set; }
        public DateTime PubDate { get; set; }
    }
}
