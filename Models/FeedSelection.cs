using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace rssreader.Models
{
    
    public class FeedSelection
    {
        public List<int> SelectedFeeds { get; set; }
    }
}
