namespace WineRatingApp.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using WineRatingApp.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WineRatingApp.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WineRatingApp.Models.ApplicationDbContext context)
        {
        }

    }
}
