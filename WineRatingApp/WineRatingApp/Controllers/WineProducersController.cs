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
    public class WineProducersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: WineProducers
        public ActionResult Index()
        {
            return View(db.WineProducers.ToList());
        }

        // GET: WineProducers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WineProducer wineProducer = db.WineProducers.Find(id);
            if (wineProducer == null)
            {
                return HttpNotFound();
            }
            return View(wineProducer);
        }

        // GET: WineProducers/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: WineProducers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "WineProducerId,WineyardName,ResponsibleProducerName,Address,City,Country,Zip,Email")] WineProducer wineProducer)
        {
            if (ModelState.IsValid)
            {
                db.WineProducers.Add(wineProducer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(wineProducer);
        }

        // GET: WineProducers/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WineProducer wineProducer = db.WineProducers.Find(id);
            if (wineProducer == null)
            {
                return HttpNotFound();
            }
            return View(wineProducer);
        }

        // POST: WineProducers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "WineProducerId,WineyardName,ResponsibleProducerName,Address,City,Country,Zip,Email")] WineProducer wineProducer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(wineProducer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(wineProducer);
        }

        // GET: WineProducers/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WineProducer wineProducer = db.WineProducers.Find(id);
            if (wineProducer == null)
            {
                return HttpNotFound();
            }
            return View(wineProducer);
        }

        // POST: WineProducers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            WineProducer wineProducer = db.WineProducers.Find(id);
            db.WineProducers.Remove(wineProducer);
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
