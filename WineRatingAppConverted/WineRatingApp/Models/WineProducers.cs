using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WineRatingApp.Models
{
    public class WineProducers
    {
        
        public virtual ICollection<WineProducer> WineProducer { get; set; }

    }
}