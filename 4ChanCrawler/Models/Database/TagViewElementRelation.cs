using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _4ChanCrawler.Models.Database
{
    public class TagViewElementRelation
    {
        [Key]
        public int Id { get; set; }

        public virtual ViewElement ViewElement { get; set; }
        public virtual Tag Tag { get; set; }
    }
}