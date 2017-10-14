using _4ChanCrawler.Models;
using _4ChanCrawler.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace _4ChanCrawler.Controllers
{
    public class AsyncController : ApiController
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public List<Category> GetCategories()
        {

            return (db.Category.ToList());
        }


    }
}
