using System.Collections.Generic;
using System.Linq;
using skeleton.Models;

namespace skeleton.Data
{
    public class WineRatingRepository : IWineRatingRepository
    {
        private List<WineRating> wineRatings = new List<WineRating>()
        {
            new WineRating {
                WineRatingId = 1,
                JudgeId = "Hans",
                Nose = 4,
                Taste = 5,
                Visuality = 5,
                WineId = 1
            },
            new WineRating {
                WineRatingId = 2,
                JudgeId = "Petter",
                Nose = 3,
                Taste = 4,
                Visuality = 3,
                WineId = 1
            },
            new WineRating {
                WineRatingId = 3,
                JudgeId = "Frans",
                Nose = 5,
                Taste = 4,
                Visuality = 6,
                WineId = 1
            },
            new WineRating {
                WineRatingId = 4,
                JudgeId = "Ola",
                Nose = 5,
                Taste = 4,
                Visuality = 4,
                WineId = 1
            }
        };
        public IList<WineRating> GetAllWineRatings()
        {
            return wineRatings;
        }

        public WineRating GetWineRatingById(int id)
        {
            return wineRatings.Find(wineRating => wineRating.WineRatingId == id);
        }

        public IList<WineRatingScore> GetScoreForWines()
        {
            List<WineRatingScore> wineScores = wineRatings
                .GroupBy(wr => wr.WineId)
                .Select(cwr => new WineRatingScore
                {
                    WineId = cwr.First().WineId,
                    Nose = cwr.Sum(x => x.Nose) / cwr.Count(),
                    Taste = cwr.Sum(x => x.Taste) / cwr.Count(),
                    Visuality = cwr.Sum(x => x.Visuality) / cwr.Count(),
                    NumberOfRatings = cwr.Count(),
                    WineLevel = SetWineLevel(cwr.Sum(x => x.Nose) / cwr.Count(), cwr.Sum(x => x.Taste) / cwr.Count(), cwr.Sum(x => x.Visuality) / cwr.Count()),
                    OverallScore = ((cwr.Sum(x => x.Nose) / cwr.Count()) + (cwr.Sum(x => x.Taste) / cwr.Count()) + (cwr.Sum(x => x.Visuality) / cwr.Count()))
                }).OrderBy(x=>x.OverallScore).ToList();

            return wineScores;
        }
        private WineLevel SetWineLevel(double nose, double taste, double visuality) {
            if (nose < 1.8 || visuality < 1.8 || taste < 5.8)
                return WineLevel.UnAcceptable;

            var overallScore = (nose + taste + visuality);

            if (overallScore >= 17)
                return WineLevel.Gold;
            if (overallScore >= 15.5)
                return WineLevel.Silver;
            if (overallScore >= 14)
                return WineLevel.Bronze;
            if (overallScore >= 12)
                return WineLevel.Great;

            return WineLevel.Acceptable;

                /*
                Gull: Minimum 17,0 poeng
Sølv: Minimum 15,5 poeng
Bronse: Minimum 14,0 poeng
Særlig utmerkelse: Minimum 12,0 poeng
Akseptabel: Se nedenfor
For å oppnå en av de ovennevnte klassifiseringer og vinen betegnes som akseptabel, skal
vinen som minimum oppnå 1,8 poeng for utseende, 1, 8 poeng for nese og 5, 8 poeng for smak.
                        */
        }
        public int AddWineRating(WineRating wineRating)
        {
            var newId = wineRatings.Max(x => x.WineRatingId) + 1;
            wineRating.WineRatingId = newId;
            wineRatings.Add(wineRating);
            return newId;
        }

        public void DeleteWineRating(int id)
        {
            wineRatings.Remove(wineRatings.Single(x => x.WineRatingId == id));
        }

    }
}
