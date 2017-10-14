using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _4ChanCrawler.Models.Database
{
    public class List
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string FK_User { get; set; }

        [ForeignKey("FK_User")]
        public virtual ApplicationUser Owner { get; set; }
        public virtual List<ViewElement> ViewElements { get; set; }

    }
}