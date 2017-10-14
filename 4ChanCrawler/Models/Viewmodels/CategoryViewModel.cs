using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _4ChanCrawler.Models.Viewmodels
{
    public class CategoryViewModel
    {
        public int CatId { get; set; }
        public int ElementCount { get; set; }
        public string Name { get; set; }
        public bool IsNSFW { get; set; }
    }
}