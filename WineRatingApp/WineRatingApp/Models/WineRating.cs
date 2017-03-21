using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WineRatingApp.Models
{
    public class WineRating
    {
        [DisplayName("Vinrating Id")]
        public int WineRatingId { get; set; }
        [DisplayName("Utseende")]
        public int Visuality { get; set; }
        [DisplayName("Nese")]
        public int Nose { get; set; }
        [DisplayName("Smak")]
        public int Taste { get; set; }
        [DisplayName("Dommer Id")]
        public string JudgeId { get; set; }
        [DisplayName("Vin Id")]
        public int WineId { get; set; }
        [ForeignKey("WineId")]
        public virtual Wine Wine { get; set; }
    }
    public class JudgeWineRatingForm
    {
        [DisplayName("Utseende")]
        public double Visuality { get; set; }
        [DisplayName("Nese")]
        public double Nose { get; set; }
        [DisplayName("Smak")]
        public double Taste { get; set; }
        [DisplayName("Vin Id")]
        public int WineId { get; set; }
        [DisplayName("Vin nivå")]
        public WineLevel WineLevel { get; set; }
        [DisplayName("Navn til dommere")]
        public string RatingName { get; set; }

        /*
Vinene klassifiseres av FND-sekretariatet og premieres etter oppnådde poeng:
Gull: Minimum 17,0 poeng
Sølv: Minimum 15,5 poeng
Bronse: Minimum 14,0 poeng
Særlig utmerkelse: Minimum 12,0 poeng
Akseptabel: Se nedenfor
For å oppnå en av de ovennevnte klassifiseringer og vinen betegnes som akseptabel, skal
vinen som minimum oppnå 1,8 poeng for utseende, 1,8 poeng for nese og 5,8 poeng for smak.
*/
    }

    public class JudgeWineRatingScore
    {
        [DisplayName("Utseende")]
        public double Visuality { get; set; }
        [DisplayName("Nese")]
        public double Nose { get; set; }
        [DisplayName("Smak")]
        public double Taste { get; set; }
        [DisplayName("Antall vurderinger")]
        public int NumberOfRatings { get; set; }
        [DisplayName("Vin nivå")]
        public WineLevel WineLevel { get; set; }
        [DisplayName("Samlet vurdering")]
        public double OverallScore { get; set; }
        [DisplayName("Navn til dommere")]
        public string RatingName { get; set; }
    }
    public class WineRatingScore
    {
        [DisplayName("Utseende")]
        public double Visuality { get; set; }
        [DisplayName("Nese")]
        public double Nose { get; set; }
        [DisplayName("Smak")]
        public double Taste { get; set; }
        [DisplayName("Antall vurderinger")]
        public int NumberOfRatings { get; set; }
        [DisplayName("Vin Id")]
        public int WineId { get; set; }
        [DisplayName("Vin nivå")]
        public WineLevel WineLevel { get; set; }
        [DisplayName("Samlet vurdering")]
        public double OverallScore { get; set; }
        [DisplayName("Navn på vin til dommere")]
        public string RatingName { get; set; }

        /*
Vinene klassifiseres av FND-sekretariatet og premieres etter oppnådde poeng:
Gull: Minimum 17,0 poeng
Sølv: Minimum 15,5 poeng
Bronse: Minimum 14,0 poeng
Særlig utmerkelse: Minimum 12,0 poeng
Akseptabel: Se nedenfor
For å oppnå en av de ovennevnte klassifiseringer og vinen betegnes som akseptabel, skal
vinen som minimum oppnå 1,8 poeng for utseende, 1,8 poeng for nese og 5,8 poeng for smak.
*/
    }

    public enum WineLevel
    {
        UnAcceptable = 0, Acceptable = 1, Great = 2, Bronze, Silver = 3, Gold = 4
    }

}
