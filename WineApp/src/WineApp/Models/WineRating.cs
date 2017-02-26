using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WineApp.Models
{
    public class WineRating
    {
        public int WineRatingId { get; set; }
        public int Visuality { get; set; }
        public int Nose { get; set; }
        public int Taste { get; set; }
        public string JudgeId { get; set; }
        public int WineId { get; set; }
        
    }
}
