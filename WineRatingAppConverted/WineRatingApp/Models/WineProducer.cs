using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WineRatingApp.Models
{
    public class WineProducer
    {
        public WineProducer()
        {
            Wines = new List<Wine>();
        }

        [Key]
        public int WineProducerId { get; set; }
        public string WineyardName { get; set; }
        public string OrganisationNumber { get; set; }
        public string ResponsibleProducerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public virtual ICollection<Wine> Wines { get; set; }
    }
}