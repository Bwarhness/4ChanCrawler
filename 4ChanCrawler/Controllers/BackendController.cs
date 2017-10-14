using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using _4ChanCrawler.Models;
using System.Data.Entity.SqlServer;

namespace _4ChanCrawler.Controllers
{
    public class BackendController : ApiController
    {
        ApplicationDbContext db = new ApplicationDbContext();
        [Route("Backend/RandomPost/{CatId}"), HttpGet]
        public string RandomPost (int CatId)
        {
            var countInDB = db.ViewElements.Where(p => p.Fk_Category == CatId).Count();
            Random ran = new Random();
            ran.Next(0, countInDB);
            var res = db.ViewElements.Where(p => p.Fk_Category == CatId).Skip(countInDB).Take(1).FirstOrDefault();
            return res.FileURL;
        }
    }
}
