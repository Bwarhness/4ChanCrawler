using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Z.EntityFramework.Plus;
namespace _4ChanCrawler.Models.Viewmodels
{
    public class IndexViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsNSFW { get; set; }
        public Database.Category CurrentCategory { get; set; }
        public List<Database.Category> AllCategories { get; set; }
        public IndexViewModel(ApplicationDbContext db, int? currentCat, bool isFavorite, bool isNSFW)
        {

            IsNSFW = isNSFW;
            if (currentCat.HasValue)
            {
                CurrentCategory = db.Category.Where(p => p.Id == currentCat.Value).FirstOrDefault();
            }
            else if (isFavorite)
            {
                IsFavorite = isFavorite;
                CurrentCategory = new Database.Category()
                {
                    Id = 0,
                    Name = "Favorite"
                };
            }
            else
            {
                CurrentCategory = db.Category.OrderBy(p => p.Name).FirstOrDefault();
            }
            AllCategories = db.Category.OrderBy(p => p.Name).ToList();

            if (AllCategories == null)
            {
                AllCategories = new List<Database.Category>();
            }
        }
    }
}