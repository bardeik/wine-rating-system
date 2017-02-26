using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WineApp.Models
{

    public class Wine
    {
        public int WineId { get; set; }
        public string RatingName { get; set; }
        public string Name { get; set; }
        public WineGroup Group { get; set; }
        public WineClass Class { get; set; }
        public WineCategory Category { get; set; }
        public int WineProducerId { get; set; }
    }
}
