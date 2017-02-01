using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WineRatingApp.Models
{
    public class WineRating
    {
        public WineRating() { }

        public int WineRatingId { get; set; }
        public int Visuality { get; set; }
        public int Nose { get; set; }
        public int Taste { get; set; }

        public string JudgeId { get; set; }

        public int WineId { get; set; }

        [ForeignKey("WineId")]
        public virtual Wine Wine { get; set; }
    }
}
