namespace skeleton.Models
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

    public class WineRatingScore
    {
        public double Visuality { get; set; }
        public double Nose { get; set; }
        public double Taste { get; set; }
        public int NumberOfRatings { get; set; }
        public int WineId { get; set; }
        public WineLevel WineLevel { get; set; }
        public double OverallScore { get; set; }

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
