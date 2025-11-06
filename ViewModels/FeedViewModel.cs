using proiect.Models;
using System.Collections.Generic;

namespace proiect.ViewModels
{
    public class FeedViewModel
    {
        public List<Post> Posts { get; set; }
        public List<Photo> Photos { get; set; }

        public List<Album> Albums { get; set; }
    }
}