using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace rssreader.Models
{
    public class Feed
    {
        public int Id { get; set; }
        public string? URL { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsChecked { get; set; }
        public ICollection<Article>? Articles { get; set; }
       
    }
}
