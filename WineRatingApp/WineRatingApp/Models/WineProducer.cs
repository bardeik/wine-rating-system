using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WineRatingApp.Models
{
    public class WineProducer
    {
        [Key]
        [DisplayName("Vinprodusent Id")]
        public int WineProducerId { get; set; }

        [DisplayName("Navn til vingård")]
        public string WineyardName { get; set; }
        [DisplayName("Organisasjonsnummer")]
        public string OrganisationNumber { get; set; }
        [DisplayName("Navn på produsent")]
        public string ResponsibleProducerName { get; set; }
        [DisplayName("Adresse")]
        public string Address { get; set; }
        [DisplayName("By")]
        public string City { get; set; }
        [DisplayName("Land")]
        public string Country { get; set; }
        [DisplayName("Postnummer")]
        public string Zip { get; set; }
        [DataType(DataType.EmailAddress)]
        [DisplayName("Epost")]
        public string Email { get; set; }

    }
}