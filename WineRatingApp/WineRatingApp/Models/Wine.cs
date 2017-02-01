using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WineRatingApp.Models
{
    public class Wine
    {
        public Wine() {
            WineRatings = new List<WineRating>();
            WineProducer = new WineProducer();
        }

        public int WineId { get; set; }
        public string RatingName { get; set; }
        public string Name { get; set; }
        public WineGroup Group { get; set; }
        public WineClass Class { get; set; }
        public WineCategory Category { get; set; }
        public virtual ICollection<WineRating> WineRatings { get; set; }

        public IEnumerable<WineProducers> WineProducers { get; set; }

        public int WineProducerId { get; set; }

        [ForeignKey("WineProducerId")]
        public virtual WineProducer WineProducer { get; set; }
    }
}