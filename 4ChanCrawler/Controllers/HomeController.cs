using _4ChanCrawler.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _4ChanCrawler.Models.Database;
using System.Net;
using System.Web.Http.Cors;
using Rotativa.MVC;
using System.Data.Entity.SqlServer;

namespace _4ChanCrawler.Controllers
{
    public class HomeController : Controller
    {
        public enum orderBy
        {
            newest = 0,
            ranked = 1,
            random = 2
        }
        class Cachedelements
        {
            public DateTime UpdatedDate { get; set; }
            public List<ViewElement> ViewElements { get; set; }
        }
        private  ApplicationDbContext db = new ApplicationDbContext();
        private static Cachedelements Cache = new Cachedelements();



        public void GetElements(int? id)
        {
            if (id.HasValue)
            {
                var Category = db.Category.Where(p => p.Id == id.Value).FirstOrDefault();
                Category.GetElements(Server.MapPath("~/Content/Files/"), db);
            }
            else
            {
                foreach (var cat in db.Category.ToList())
                {
                    cat.GetElements(Server.MapPath("~/Content/Files/"), db);

                }
            }
            UpdateCache();

        }
        public List<Category> APIGetElements()
        {
            return db.Category.ToList();
        }
        public void CleanUp()
        {
            List<ViewElement> Elements = new List<ViewElement>();
            foreach (var item in db.ViewElements.ToList())
            {
                HttpWebResponse response = null;

                try
                {
                    if (item.FileURL.Contains("http"))
                    {


                        var request = (HttpWebRequest)WebRequest.Create(item.FileURL);
                        request.Method = "HEAD";
                        response = (HttpWebResponse)request.GetResponse();

                    }
                    else
                    {

                        var url = Server.MapPath("~/Content/Files/") + item.FileURL;
                        var request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "HEAD";
                        response = (HttpWebResponse)request.GetResponse();

                    }

                }
                catch (Exception)
                {

                    Elements.Add(item);

                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }


                }
                DeleteVideos(Elements);
            }
        }
        public void DeleteVideos(List<ViewElement> ViewElements)
        {
            for (int i = 0; i < ViewElements.Count; i++)
            {
                var element = ViewElements[i];
                if (element != null)
                {

                if (Cache.ViewElements != null)
                {
                    Cache.ViewElements.Remove(element);

                }
                if (element.IsLocal)
                {
                    System.IO.File.Delete(Server.MapPath("~/Content/Files/") + element.FileURL);

                }
                }

            }
            db.ViewElements.RemoveRange(ViewElements);
            db.SaveChanges();
        }
        public void UpdateCache()
        {
            Cache.ViewElements = db.ViewElements.ToList();
            Cache.UpdatedDate = DateTime.Today;
        }


        public string DeleteVideo(int id)
        {
            var videoToDelete = db.ViewElements.Find(id);
            videoToDelete.IsNSFW = true;
            db.ViewElements.AddOrUpdate(videoToDelete);
            db.SaveChanges();
            return "Dinmor";

        }


        [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
        public object GetPostNew(int CategoryID, int SkipAmount = 0, orderBy order = orderBy.newest)
        {

            bool KeepLooping = true;
            bool Redirect = false;
            List<Models.Database.ViewElement> ElementsToRemove = new List<Models.Database.ViewElement>();
            ViewElement ElementToShow = new ViewElement();
            while (KeepLooping)
            {

                SkipAmount = SkipAmount > Cache.ViewElements.Where(p => p.Fk_Category == CategoryID).Count() ? 0 : SkipAmount;
                KeepLooping = false;

                switch (order)
                {
                    case orderBy.newest:
                        ElementToShow = Cache.ViewElements.Where(p => p.Fk_Category == CategoryID).OrderByDescending(p => p.Id).Skip(SkipAmount).Take(1).FirstOrDefault();
                        break;
                    case orderBy.random:
                        ElementToShow = Cache.ViewElements.Where(p => p.Fk_Category == CategoryID).OrderBy(p => Guid.NewGuid()).FirstOrDefault();
                        break;
                    case orderBy.ranked:
                        ElementToShow = Cache.ViewElements.Where(p => p.Fk_Category == CategoryID).OrderByDescending(p =>
                        p.Ratings.Where(S => S.Relation == Relation.Favorite ||
                        S.Relation == Relation.Like).Count()).Skip(SkipAmount).Take(1).FirstOrDefault();
                        break;
                    default:
                        ElementToShow = Cache.ViewElements.Where(p => p.Fk_Category == CategoryID).OrderBy(p => Guid.NewGuid()).FirstOrDefault();

                        break;
                } 
                #region Check If file Exists
                HttpWebResponse response = null;

                try
                {
                    if (ElementToShow.FileURL.Contains("http"))
                    {


                        var request = (HttpWebRequest)WebRequest.Create(ElementToShow.FileURL);
                        request.Method = "HEAD";
                        response = (HttpWebResponse)request.GetResponse();

                    }
                    else
                    {

                        var url = Server.MapPath("~/Content/Files/") + ElementToShow.FileURL;
                        var request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "HEAD";
                        response = (HttpWebResponse)request.GetResponse();

                    }

                }
                catch (Exception)
                {

                    ElementsToRemove.Add(ElementToShow);
                    SkipAmount++;
                    KeepLooping = true;

                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }


                }


                #endregion
            }
            if (ElementsToRemove.Any())
            {

                DeleteVideos(ElementsToRemove);
            }
            return JsonConvert.SerializeObject(new
            {
                FileUrl = ElementToShow.FileURL,
                PostID = ElementToShow.Id,
                SkipAmount = SkipAmount

            });

        }
        public string GetNextPost(int? Id, int SkipAmount = 0, bool GetLast = false, bool isFavorite = false, bool getRandom = false, bool isNSFW = false)
        {

            var CurrentUserID = User.Identity.GetUserId();

            Models.Viewmodels.JsonModel JsonReturnModel = new Models.Viewmodels.JsonModel();
            Models.Database.ViewElement ViewElement = new Models.Database.ViewElement();

            if (Id.HasValue == false)
            {
                Id = db.Category.FirstOrDefault().Id;
            }
            bool loop = true;
            List<Models.Database.ViewElement> ElementsToRemove = new List<Models.Database.ViewElement>();
            while (loop)
            {
                IQueryable<Models.Database.ViewElement> viewElements;


                if (isFavorite)
                {
                    viewElements = db.Ratings.Where(p => p.FK_User == CurrentUserID && p.Relation == Models.Database.Relation.Favorite).Select(p => p.Element);
                    JsonReturnModel.IsFavorite = true;
                }
                else
                {
                    var res = db.ViewElements.Where(p => p.Fk_Category == Id).ToList();
                    viewElements = db.ViewElements.Where(p => p.Fk_Category == Id);
                }

                if (viewElements.Count() <= SkipAmount || SkipAmount < 0)
                {
                    SkipAmount = 0;
                    JsonReturnModel.Reset = true;
                }

                    viewElements = viewElements.Where(p => p.IsNSFW == isNSFW);
                if (getRandom)
                {
                    Random rnd = new Random();
                    var nextvideo = rnd.Next(0, viewElements.Count());
                    ViewElement = viewElements.OrderBy(p => p.Id).Skip(nextvideo).Take(1).FirstOrDefault();
                }
                else
                {
                    //ViewElement = ViewElements.OrderByDescending(p => p.Ratings.Where(s => s.Relation == Models.Database.Relation.Favorite || s.Relation == Models.Database.Relation.Like).Count()).ThenByDescending(p => p.CreatedDate).Skip(/*ran.Next(0, countInDB)*/SkipAmount).Take(1).FirstOrDefault();
                    ViewElement = viewElements.OrderByDescending(p => p.CreatedDate).Skip(/*ran.Next(0, countInDB)*/SkipAmount).Take(1).FirstOrDefault();
                }



                //Check if there is a rating from current user.
                if (ViewElement != null && db.Ratings.Where(p => p.FK_User == CurrentUserID && p.FK_Element == ViewElement.Id).Any())
                {
                    var rating = db.Ratings.Where(p => p.FK_User == CurrentUserID && p.FK_Element == ViewElement.Id).FirstOrDefault();
                    JsonReturnModel.Rating = (int)rating.Relation;
                }
                try
                {

                    using (var wc = new System.Net.WebClient())
                        if (ViewElement.FileURL.Contains("http"))
                        {
                            wc.DownloadString(ViewElement.FileURL);

                        }
                        else
                        {
                            wc.DownloadString(Server.MapPath("~/Content/Files/") + ViewElement.FileURL);

                        }
                    loop = false;
                }
                catch (Exception)
                {

                    ElementsToRemove.Add(ViewElement);
                    SkipAmount++;

                }


                if (viewElements.Count() == 0)
                {
                    loop = false;
                }
            }
            if (ElementsToRemove.Count > 0)
            {
                db.ViewElements.RemoveRange(ElementsToRemove);
                db.SaveChanges();
            }
            JsonReturnModel.Likes = db.Ratings.Count(p => p.FK_Element == ViewElement.Id && (p.Relation == Relation.Favorite || p.Relation == Relation.Like));
            JsonReturnModel.Dislikes = db.Ratings.Count(p => p.FK_Element == ViewElement.Id && p.Relation == Relation.Dislike);

            JsonReturnModel.ViewElement = ViewElement;

            JsonReturnModel.ViewElement.Category.ViewElements = new List<Models.Database.ViewElement>();

            JsonReturnModel.ViewElement.Ratings = new List<Models.Database.Rating>();
            JsonReturnModel.ViewElement.Ratings = null;
            JsonReturnModel.ViewElement.Lists = new List<Models.Database.List>();
            JsonReturnModel.ViewElement.Lists = null;


            var bla = JsonConvert.SerializeObject(JsonReturnModel, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
            return bla;
        }
        [HttpPost]
        // ReSharper disable once MethodTooLong
        public void RatePost(int Id, Relation Relation)
        {

            var CurrentUserId = User.Identity.GetUserId();
            var ExistingRating = db.Ratings.Where(p => p.FK_User == CurrentUserId && p.FK_Element == Id).FirstOrDefault();
            if (ExistingRating != null) // if rating exists
            {
                if (ExistingRating.Relation == Relation)//Deletes rating if user wants to undo it.
                {
                    db.Ratings.Remove(ExistingRating);
                }
                else//changes rating
                {
                    ExistingRating.Relation = Relation;
                    db.Ratings.AddOrUpdate(ExistingRating);
                }

            }
            else //Create new rating.
            {
                var newRating = new Models.Database.Rating()
                {
                    FK_Element = Id,
                    FK_User = CurrentUserId,
                    Relation = Relation
                };
                db.Ratings.Add(newRating);

                switch (newRating.Relation)
                {
                    case Relation.Favorite:
                    case Relation.Like:
                        var Element = db.ViewElements.Where(p => p.Id == Id).FirstOrDefault();
                        if (Element.FileType == Models.Database.FileType.WebM)
                        {
                            Element.DownloadFile(Server.MapPath("~/Content/Files/"));
                            db.ViewElements.AddOrUpdate(Element);
                        }
                        break;
                }
            }
            db.SaveChanges();
        }

        public ActionResult Index(int? Id, bool isFavorite = false, bool isNSFW = false)
        {
            return View(new Models.Viewmodels.IndexViewModel(db, Id, isFavorite, isNSFW));
        }
        public ActionResult PrintIndex()
{
        return new ActionAsPdf("Category", new { name = "Giorgio" }) { FileName = "Test.pdf" };
}

        public ActionResult Category()
        {

            if (Cache.UpdatedDate != DateTime.Today)
            {
                UpdateCache();
            }

            List<Models.Viewmodels.CategoryViewModel> VM = new List<Models.Viewmodels.CategoryViewModel>();

            VM = db.Category.Select(p => new Models.Viewmodels.CategoryViewModel() {
                Name = p.Name,
                IsNSFW = p.IsNSFW,
                CatId = p.Id,
            }).ToList();
            foreach (var item in VM)
            {
                var res = Cache.ViewElements.Where(p => p.Fk_Category == item.CatId).ToList();
                if (res != null)
                {
                    item.ElementCount = res.Count();
                }

            }
            return View(VM);

        }

        public ActionResult CreateCat(int? id)
        {
            Category res = new Category();
            if (id.HasValue)
            {
                res = db.Category.Find(id);
            }
            return View(res);
        }
        [HttpPost]
        public ActionResult CreateCat(Category NewCat)
        {
            db.Category.AddOrUpdate(NewCat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}
