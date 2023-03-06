using Microsoft.EntityFrameworkCore;
using rssreader.Models;

namespace rssreader.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Feed>? Feeds { get; set; }
        public DbSet<Article>? Articles { get; set; }
        //public DbSet<FeedSelection> FeedSelection { get; set; }
    }
}
