using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _4ChanCrawler.Models.Database
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        public Relation Relation { get; set; }
        public string FK_User { get; set; }
        public int FK_Element { get; set; }
        [ForeignKey("FK_User")]
        public virtual ApplicationUser User { get; set; }
        [ForeignKey("FK_Element")]
        public virtual ViewElement Element { get; set; }
    }
    public enum Relation
    {
        Dislike = 0,
        Like = 10,
        Favorite = 20
    }
}