using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _4ChanCrawler.Models.Viewmodels
{
    public class JsonModel
    {
        public Database.ViewElement ViewElement { get; set; }
        public bool Reset { get; set; }
        public int? Rating { get; set; }
        public bool IsFavorite { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}