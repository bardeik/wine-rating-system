using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WineRatingApp.Models
{
    public class Wine
    {
        [DisplayName("Vin Id")]
        public int WineId { get; set; }

        [DisplayName("Navn som dommere ser")]
        public string RatingName { get; set; }

        [DisplayName("Navn på vin")]
        public string Name { get; set; }

        [DisplayName("Gruppe")]
        public WineGroup Group { get; set; }

        [DisplayName("Klasse")]
        public WineClass Class { get; set; }

        [DisplayName("Kategori")]
        public WineCategory Category { get; set; }

        [DisplayName("Vinprodusent Id")]
        public int WineProducerId { get; set; }
    }
}