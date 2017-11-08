using _4ChanCrawler.Models.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;

namespace _4ChanCrawler.Models.Database
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string SearchParams { get; set; }
        [Required]
        public string BoardsArray { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool AcceptPics { get; set; }
        public bool IsNSFW { get; set; }
        public bool AcceptVids { get; set; }
        public virtual List<ViewElement> ViewElements { get; set; }
        public void GetElements(string installPath, ApplicationDbContext db)
        {
            List<ViewElement> ElementsToSave = new List<ViewElement>();
            using (var client = new WebClient())
            {


                string[] Boards = BoardsArray.Split(';');//array of boards. Set in boards like "b", and so on
                foreach (var board in Boards.Where(p => !string.IsNullOrEmpty(p)))
                {

                    string json = client.DownloadString("http://a.4cdn.org/" + board.ToLower() + "/catalog.json");

                    List<JToken> MatchingThreads = new List<JToken>();
                    JArray Content = JArray.Parse(json);
                    for (int i = 0; i < Content.Count; i++)
                    {
                        JObject CurrentPage = (JObject)Content[i];
                        List<string> searchParams = SearchParams.Split(';').Where(p => string.IsNullOrEmpty(p) == false).ToList(); //Her indtaster vi de ønskede threads vi vil finde. Den søger i teksten og ser om den container det vi indtaster, f.eks "YLYL"

                        foreach (var param in searchParams)
                        {
                            var bla = CurrentPage.SelectToken("threads")
                            .Where(x => x.SelectToken("com") != null || x.SelectToken("sub") != null)
                            .Where(p => (p.SelectToken("com") != null && p.SelectToken("com").ToString().ToLower().Contains(param.ToLower()))
                            || (p.SelectToken("sub") != null && p.SelectToken("sub").ToString().ToLower().Contains(param.ToLower()))).ToList();

                            if (MatchingThreads.Where(p => p.ToString() == bla.ToString()).Any() == false)
                            {
                                MatchingThreads.AddRange(bla);

                            }
                        };
                        

                    }
                    foreach (var item in MatchingThreads) //We use cat for storing in folders in the future
                    {
                            string downloadUrl = "";
                            string baseurl = "http://i.4cdn.org/" + board.ToLower() + "/";
                            if ((int)item.SelectToken("replies") > 1)
                            {
                                string thread = client.DownloadString("http://a.4cdn.org/" + board.ToLower() + "/thread/" + item.SelectToken("no").ToString() + ".json");
                                var JThread = JObject.Parse(thread);
                                JArray AllReplies = (JArray)JThread.SelectToken("posts");
                                foreach (var Reply in AllReplies.Where(p => p.SelectToken("ext") != null).Reverse())
                                {
                                    if (((Reply.SelectToken("ext").ToString().ToLower() != ".webm" && AcceptPics == true) || (Reply.SelectToken("ext").ToString().ToLower() == ".webm" && AcceptVids == true)))
                                    {
                                        var md5 = Reply.SelectToken("md5").ToString();
                                        if ((!db.ViewElements.Where(p => p.MD5.ToLower() == md5.ToLower() && p.Fk_Category == this.Id).Any()) && !ElementsToSave.Where(p => p.MD5.ToLower() == md5.ToLower()).Any())
                                        {//Check if file is existing, and continues if it doesnt.

                                            var IsVideo = Reply.SelectToken("ext").ToString().ToLower() == ".webm";
                                            downloadUrl = baseurl + Reply.SelectToken("tim").ToString() + Reply.SelectToken("ext").ToString();


                                            System.IO.Directory.CreateDirectory(installPath);

                                            ViewElement ViewElement = new ViewElement()
                                            {
                                                MD5 = Reply.SelectToken("md5").ToString(),
                                                Name = item.SelectToken("tim").ToString(),
                                                Fk_Category = this.Id,
                                                FileType = FileType.picture,
                                                FileURL = downloadUrl,
                                                CreatedDate = DateTime.Now,
                                                IsNSFW = IsNSFW
                                                

                                            };

                                            if (!IsVideo)
                                            {
                                                client.DownloadFile(downloadUrl, installPath + Reply.SelectToken("tim").ToString() + Reply.SelectToken("ext").ToString());
                                                ViewElement.FileURL = Reply.SelectToken("tim").ToString() + Reply.SelectToken("ext").ToString();
                                            }
                                            else
                                            {
                                                ViewElement.FileType = FileType.WebM;
                                            }
                                            ElementsToSave.Add(ViewElement);
                                        }

                                    }




                                }
                            }
                        

                        //JArray replies = item.SelectToken
                    }

                }
            }
            
            db.ViewElements.AddRange(ElementsToSave);
            this.LastUpdated = DateTime.Now;
            db.Category.AddOrUpdate(this);
            db.SaveChanges();
        }
    }

}