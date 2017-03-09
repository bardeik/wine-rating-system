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
