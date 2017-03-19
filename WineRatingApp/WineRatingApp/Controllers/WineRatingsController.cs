using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WineRatingApp.Models;

namespace WineRatingApp.Controllers
{
    public class WineRatingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: WineRatings
        public ActionResult Index()
        {
            return View(db.WineRatings.ToList());
        }

        // GET: WineRatings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WineRating wineRating = db.WineRatings.Find(id);
            if (wineRating == null)
            {
                return HttpNotFound();
            }
            return View(wineRating);
        }

        // GET: WineRatings/ScoreDetails
        public ActionResult ScoreDetails()
        {
            var wineRatingList = db.WineRatings.ToList();
            var wineList = db.Wines.ToList();

            List<WineRatingScore> wineScores = wineRatingList
                .GroupBy(wr => wr.WineId)
                .Select(cwr => new WineRatingScore
                {
                    WineId = cwr.First().WineId,
                    RatingName = wineList.Where(x => x.WineId == cwr.First().WineId).FirstOrDefault().RatingName,
                    Nose = cwr.Sum(x => x.Nose) / cwr.Count(),
                    Taste = cwr.Sum(x => x.Taste) / cwr.Count(),
                    Visuality = cwr.Sum(x => x.Visuality) / cwr.Count(),
                    NumberOfRatings = cwr.Count(),
                    WineLevel = SetWineLevel(cwr.Sum(x => x.Nose) / cwr.Count(), cwr.Sum(x => x.Taste) / cwr.Count(), cwr.Sum(x => x.Visuality) / cwr.Count()),
                    OverallScore = ((cwr.Sum(x => x.Nose) / cwr.Count()) + (cwr.Sum(x => x.Taste) / cwr.Count()) + (cwr.Sum(x => x.Visuality) / cwr.Count()))
                }).OrderBy(x => x.OverallScore).ToList();

            return View(wineScores.ToList());
        }
    
        private WineLevel SetWineLevel(double nose, double taste, double visuality)
        {
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

        // GET: WineRatings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: WineRatings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "WineRatingId,JudgeId,WineId,Visuality,Nose,Taste")] WineRating wineRating)
        {
            if (ModelState.IsValid)
            {
                db.WineRatings.Add(wineRating);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(wineRating);
        }

        // GET: WineRatings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WineRating wineRating = db.WineRatings.Find(id);
            if (wineRating == null)
            {
                return HttpNotFound();
            }
            return View(wineRating);
        }

        // POST: WineRatings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "WineRatingId,JudgeId,WineId,Visuality,Nose,Taste")] WineRating wineRating)
        {
            if (ModelState.IsValid)
            {
                db.Entry(wineRating).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(wineRating);
        }

        // GET: WineRatings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WineRating wineRating = db.WineRatings.Find(id);
            if (wineRating == null)
            {
                return HttpNotFound();
            }
            return View(wineRating);
        }

        // POST: WineRatings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            WineRating wineRating = db.WineRatings.Find(id);
            db.WineRatings.Remove(wineRating);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
