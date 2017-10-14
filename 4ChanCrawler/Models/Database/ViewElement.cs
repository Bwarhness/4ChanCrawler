using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Web;

namespace _4ChanCrawler.Models.Database
{
    public class ViewElement
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MD5 { get; set; }
        public string Name { get; set; }
        public int Fk_Category { get; set; }
        public string FileURL { get; set; }
        public FileType FileType { get; set; }
        public bool IsLocal { get; set; }
        public bool IsNSFW { get; set; }
        [ForeignKey("Fk_Category")]
        public virtual Category Category { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual List<List> Lists { get; set; }


        public void DownloadFile(string installPath)
        {
            using (var client = new WebClient())
            {
                var newLocation = Guid.NewGuid().ToString().Replace("-", "") + ".webm";
                client.DownloadFile(FileURL, installPath +  newLocation);
                this.FileURL = newLocation;
                this.IsLocal = true;
            }
        }
    }


    public enum FileType
    {
        picture = 0,
        WebM = 10,
    }
}