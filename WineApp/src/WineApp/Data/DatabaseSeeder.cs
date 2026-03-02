using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using WineApp.Models;
using WineApp.Services;

namespace WineApp.Data;

public class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var roleManager = services.GetRequiredService<RoleManager<MongoIdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Always seed roles (safe in all environments)
        await SeedRolesAsync(roleManager);

        // Seed a production admin from environment variables when running outside
        // Development. Set ADMIN_EMAIL and ADMIN_PASSWORD as Fly.io secrets (or
        // any environment variable) before the first deploy.
        // Nothing happens if either variable is missing or the user already exists.
        await SeedProductionAdminAsync(userManager);

        // Sample data with well-known passwords must never run in Production.
        // Set ASPNETCORE_ENVIRONMENT=Production (or any non-Development value) to skip.
        if (!env.IsDevelopment())
            return;

        // Seed admin and viewer users
        await SeedAdminAndViewerUsersAsync(userManager);

        // Seed judges
        await SeedJudgesAsync(userManager);

        // Seed event with producers, wines and ratings
        await SeedEventDataAsync(services, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<MongoIdentityRole<Guid>> roleManager)
    {
        foreach (var role in new[] { "Admin", "Judge", "Viewer", "WineProducer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new MongoIdentityRole<Guid>(role));
        }
    }

    /// <summary>
    /// Creates an admin account from ADMIN_EMAIL and ADMIN_PASSWORD environment variables.
    /// Safe to call in Production — skips silently if either variable is absent or the
    /// account already exists.
    /// </summary>
    private static async Task SeedProductionAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "Viewer");
        }
    }

    private static async Task SeedAdminAndViewerUsersAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync("admin@wineapp.com") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@wineapp.com",
                Email = "admin@wineapp.com",
                DisplayName = "Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "Viewer");
        }

        if (await userManager.FindByEmailAsync("viewer@wineapp.com") is null)
        {
            var viewer = new ApplicationUser
            {
                UserName = "viewer@wineapp.com",
                Email = "viewer@wineapp.com",
                DisplayName = "Gjest",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(viewer, "Viewer123!");
            await userManager.AddToRoleAsync(viewer, "Viewer");
        }
    }

    private static async Task SeedJudgesAsync(UserManager<ApplicationUser> userManager)
    {
        var judgeUsers = await userManager.GetUsersInRoleAsync("Judge");
        if (judgeUsers.Count == 0)
        {
            foreach (var name in new[] { "Frans", "Hans", "Ola", "Petter" })
            {
                var email = $"{name.ToLower()}@wineapp.com";
                var judgeUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = name,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(judgeUser, "Judge123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(judgeUser, "Judge");
                    await userManager.AddToRoleAsync(judgeUser, "Viewer");
                }
            }
        }
    }

    private static async Task SeedEventDataAsync(IServiceProvider services, UserManager<ApplicationUser> userManager)
    {
        var wineProducerRepo = services.GetRequiredService<IWineProducerRepository>();
        var wineRepo = services.GetRequiredService<IWineRepository>();
        var wineRatingRepo = services.GetRequiredService<IWineRatingRepository>();
        var eventRepo = services.GetRequiredService<IEventRepository>();
        var wineNumberService = services.GetRequiredService<IWineNumberService>();

        // Seed default event if none exist
        if ((await eventRepo.GetAllEventsAsync()).Count == 0)
        {
            var currentYear = DateTime.Now.Year;
            var defaultEvent = new Event
            {
                EventId = ObjectId.GenerateNewId().ToString(),
                Name = $"Norsk Vinskue {currentYear}",
                Year = currentYear,
                RegistrationStartDate = new DateTime(currentYear, 1, 1),
                RegistrationEndDate = new DateTime(currentYear, 8, 15),
                PaymentDeadline = new DateTime(currentYear, 8, 31),
                DeliveryDeadline = new DateTime(currentYear, 9, 15),
                FeePerWine = 500m,
                BankName = "Landkreditt Bank",
                AccountNumber = "1234.56.78901",
                IBAN = "NO93 1234 5678 901",
                BIC = "NOLILNOX",
                OrganizationNumber = "987654321",
                DeliveryAddressNorway = "Norsk Vinforening\nPostboks 123\n0123 Oslo",
                ImporterInfoNordic = "Kontakt importør AS for nordiske viner\nTlf: +47 12345678",
                GoldThreshold = 17.0m,
                SilverThreshold = 15.5m,
                BronzeThreshold = 14.0m,
                SpecialMeritThreshold = 12.0m,
                AdjustedGoldThreshold = 15.0m,
                AdjustedSilverThreshold = 14.0m,
                AdjustedBronzeThreshold = 13.0m,
                AdjustedSpecialMeritThreshold = 11.5m,
                AppearanceGateValue = 1.8m,
                NoseGateValue = 1.8m,
                TasteGateValue = 5.8m,
                OutlierThreshold = 4.0m,
                UseAdjustedThresholds = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            await eventRepo.AddEventAsync(defaultEvent);

            // Seed producers and wines for this event
            await SeedProducersAndWinesAsync(
                userManager,
                wineProducerRepo,
                wineRepo,
                wineRatingRepo,
                wineNumberService,
                defaultEvent);
        }
    }

    private static async Task SeedProducersAndWinesAsync(
        UserManager<ApplicationUser> userManager,
        IWineProducerRepository wineProducerRepo,
        IWineRepository wineRepo,
        IWineRatingRepository wineRatingRepo,
        IWineNumberService wineNumberService,
        Event defaultEvent)
    {
        if ((await wineProducerRepo.GetAllWineProducersAsync()).Count > 0)
            return;

        // Helper: create ApplicationUser with WineProducer role, return UserId
        async Task<string?> CreateProducerUserAsync(string email, string displayName)
        {
            var u = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                EmailConfirmed = true
            };
            var r = await userManager.CreateAsync(u, "Producer123!");
            if (!r.Succeeded) return null;
            await userManager.AddToRoleAsync(u, "WineProducer");
            await userManager.AddToRoleAsync(u, "Viewer");
            return u.Id.ToString();
        }

        // Create producer users
        var p1UserId = await CreateProducerUserAsync("oslo@wineapp.com", "Oslo Vest Wines AS");
        var p2UserId = await CreateProducerUserAsync("grimstad@wineapp.com", "Grimstad Vin og Vann AS");
        var p3UserId = await CreateProducerUserAsync("techwine@wineapp.com", "Tech Wine AS");
        var p4UserId = await CreateProducerUserAsync("nordic@wineapp.com", "Nordic Vineyard AB");

        // Create producers
        var producers = CreateProducers(p1UserId, p2UserId, p3UserId, p4UserId);
        foreach (var producer in producers)
        {
            await wineProducerRepo.AddWineProducerAsync(producer);
        }

        // Create wines
        if ((await wineRepo.GetAllWinesAsync()).Count == 0)
        {
            var wines = CreateWines(producers, defaultEvent.EventId);
            foreach (var wine in wines)
            {
                await wineRepo.AddWineAsync(wine);
            }

            // Assign wine numbers to paid wines
            await wineNumberService.AssignWineNumbersAsync(defaultEvent.EventId);

            // Seed ratings
            await SeedRatingsAsync(wineRatingRepo, wines);
        }
    }

    private static List<WineProducer> CreateProducers(
        string? p1UserId, 
        string? p2UserId, 
        string? p3UserId, 
        string? p4UserId)
    {
        return
        [
            new WineProducer 
            { 
                WineProducerId = ObjectId.GenerateNewId().ToString(), 
                MemberNumber = "001", 
                UserId = p1UserId, 
                Address = "Vinveien 1", 
                City = "Oslo", 
                Country = "Norge", 
                Email = "oslo@wineapp.com", 
                OrganisationNumber = "111122223333445", 
                ResponsibleProducerName = "Kari Nordmann", 
                WineyardName = "Oslo Vest Wines AS", 
                Zip = "0125", 
                Phone = "+47 12345678" 
            },
            new WineProducer 
            { 
                WineProducerId = ObjectId.GenerateNewId().ToString(), 
                MemberNumber = "002", 
                UserId = p2UserId, 
                Address = "Drueveien 15", 
                City = "Grimstad", 
                Country = "Norge", 
                Email = "grimstad@wineapp.com", 
                OrganisationNumber = "111122234567890", 
                ResponsibleProducerName = "Per Hansen", 
                WineyardName = "Grimstad Vin og Vann AS", 
                Zip = "4525", 
                Phone = "+47 23456789" 
            },
            new WineProducer 
            { 
                WineProducerId = ObjectId.GenerateNewId().ToString(), 
                MemberNumber = "003", 
                UserId = p3UserId, 
                Address = "Agder Allé 21", 
                City = "Kristiansand", 
                Country = "Norge", 
                Email = "techwine@wineapp.com", 
                OrganisationNumber = "222222223333445", 
                ResponsibleProducerName = "Bård Eik", 
                WineyardName = "Tech Wine AS", 
                Zip = "4631", 
                Phone = "+47 34567890" 
            },
            new WineProducer 
            { 
                WineProducerId = ObjectId.GenerateNewId().ToString(), 
                MemberNumber = "N01", 
                UserId = p4UserId, 
                Address = "Vingatan 45", 
                City = "Stockholm", 
                Country = "Sverige", 
                Email = "nordic@wineapp.com", 
                OrganisationNumber = "SE123456789", 
                ResponsibleProducerName = "Erik Svensson", 
                WineyardName = "Nordic Vineyard AB", 
                Zip = "12345", 
                Phone = "+46 123456789" 
            }
        ];
    }

    private static List<Wine> CreateWines(List<WineProducer> producers, string eventId)
    {
        var wines = new List<Wine>()
        {
            // Producer 1 - Oslo (A1 wines, Vinbonde eligible)
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Rondo Reserve", 
                RatingName = "Test-001", 
                WineProducerId = producers[0].WineProducerId, 
                Category = WineCategory.Rodvin, 
                Class = WineClass.Eldre, 
                Group = WineGroup.A1, 
                Vintage = 2022, 
                AlcoholPercentage = 13.5m, 
                Country = "Norge", 
                IsVinbonde = true, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Solaris Blanc", 
                RatingName = "Test-002", 
                WineProducerId = producers[0].WineProducerId, 
                Category = WineCategory.Hvitvin, 
                Class = WineClass.Unge, 
                Group = WineGroup.A1, 
                Vintage = 2023, 
                AlcoholPercentage = 11.5m, 
                Country = "Norge", 
                IsVinbonde = true, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            
            // Producer 2 - Grimstad (Mix of groups)
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Phoenix Rose", 
                RatingName = "Test-003", 
                WineProducerId = producers[1].WineProducerId, 
                Category = WineCategory.Rosevin, 
                Class = WineClass.Unge, 
                Group = WineGroup.A1, 
                Vintage = 2023, 
                AlcoholPercentage = 12.0m, 
                Country = "Norge", 
                IsVinbonde = false, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Regent Rouge", 
                RatingName = "Test-004", 
                WineProducerId = producers[1].WineProducerId, 
                Category = WineCategory.Rodvin, 
                Class = WineClass.Eldre, 
                Group = WineGroup.B, 
                Vintage = 2021, 
                AlcoholPercentage = 13.0m, 
                Country = "Norge", 
                IsVinbonde = true, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            
            // Producer 3 - Kristiansand (Trial grapes)
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Experimental Red", 
                RatingName = "Test-005", 
                WineProducerId = producers[2].WineProducerId, 
                Category = WineCategory.Rodvin, 
                Class = WineClass.Unge, 
                Group = WineGroup.C, 
                Vintage = 2023, 
                AlcoholPercentage = 12.5m, 
                Country = "Norge", 
                IsVinbonde = false, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Greenhouse White", 
                RatingName = "Test-006", 
                WineProducerId = producers[2].WineProducerId, 
                Category = WineCategory.Hvitvin, 
                Class = WineClass.Unge, 
                Group = WineGroup.D, 
                Vintage = 2023, 
                AlcoholPercentage = 10.5m, 
                Country = "Norge", 
                IsVinbonde = false, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            
            // Producer 4 - Nordic guest (A2)
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Swedish Summer", 
                RatingName = "Test-007", 
                WineProducerId = producers[3].WineProducerId, 
                Category = WineCategory.Hvitvin, 
                Class = WineClass.Unge, 
                Group = WineGroup.A2, 
                Vintage = 2023, 
                AlcoholPercentage = 11.0m, 
                Country = "Sverige", 
                IsVinbonde = false, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            
            // Additional wines for variety (some unpaid for testing)
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Dessert Delight", 
                RatingName = "Test-008", 
                WineProducerId = producers[0].WineProducerId, 
                Category = WineCategory.Dessertvin, 
                Class = WineClass.Eldre, 
                Group = WineGroup.A1, 
                Vintage = 2020, 
                AlcoholPercentage = 15.0m, 
                Country = "Norge", 
                IsVinbonde = true, 
                EventId = eventId, 
                IsPaid = false, 
                SubmissionDate = DateTime.UtcNow 
            },
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Sparkling Joy", 
                RatingName = "Test-009", 
                WineProducerId = producers[1].WineProducerId, 
                Category = WineCategory.Mousserendevin, 
                Class = WineClass.Unge, 
                Group = WineGroup.A1, 
                Vintage = 2022, 
                AlcoholPercentage = 12.5m, 
                Country = "Norge", 
                IsVinbonde = false, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            },
            new Wine 
            { 
                WineId = ObjectId.GenerateNewId().ToString(), 
                Name = "Mulled Magic", 
                RatingName = "Test-010", 
                WineProducerId = producers[2].WineProducerId, 
                Category = WineCategory.Hetvin, 
                Class = WineClass.Unge, 
                Group = WineGroup.A1, 
                Vintage = 2023, 
                AlcoholPercentage = 8.5m, 
                Country = "Norge", 
                IsVinbonde = false, 
                EventId = eventId, 
                IsPaid = true, 
                SubmissionDate = DateTime.UtcNow 
            }
        };

        // Set grape blends
        wines[0].GrapeBlend.Add("Rondo", 100m);
        wines[1].GrapeBlend.Add("Solaris", 100m);
        wines[2].GrapeBlend.Add("Phoenix", 70m);
        wines[2].GrapeBlend.Add("Solaris", 30m);
        wines[3].GrapeBlend.Add("Regent", 100m);
        wines[4].GrapeBlend.Add("Leon Millot", 60m);
        wines[4].GrapeBlend.Add("Rondo", 40m);
        wines[5].GrapeBlend.Add("Phoenix", 100m);
        wines[6].GrapeBlend.Add("Solaris", 100m);
        wines[7].GrapeBlend.Add("Rondo", 50m);
        wines[7].GrapeBlend.Add("Regent", 50m);
        wines[8].GrapeBlend.Add("Solaris", 100m);
        wines[9].GrapeBlend.Add("Rondo", 100m);

        return wines;
    }

    private static async Task SeedRatingsAsync(IWineRatingRepository wineRatingRepo, List<Wine> wines)
    {
        if ((await wineRatingRepo.GetAllWineRatingsAsync()).Count > 0)
            return;

        // Wine 1 - Excellent ratings (should get Gold)
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 2.8m, Nose = 3.5m, Taste = 11.0m, Comment = "Utmerket balanse", WineId = wines[0].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 2.5m, Nose = 3.2m, Taste = 11.5m, Comment = "Flott vin", WineId = wines[0].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 2.7m, Nose = 3.8m, Taste = 11.2m, Comment = "Veldig god", WineId = wines[0].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 2.6m, Nose = 3.6m, Taste = 11.3m, Comment = "Imponerende", WineId = wines[0].WineId, RatingDate = DateTime.UtcNow });

        // Wine 2 - Good ratings (should get Silver)
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 2.3m, Nose = 2.8m, Taste = 10.0m, Comment = "God friskhet", WineId = wines[1].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 2.2m, Nose = 2.9m, Taste = 10.5m, Comment = "Fin vin", WineId = wines[1].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 2.4m, Nose = 3.0m, Taste = 10.2m, Comment = "Behagelig", WineId = wines[1].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 2.3m, Nose = 2.7m, Taste = 10.3m, Comment = "Harmonisk", WineId = wines[1].WineId, RatingDate = DateTime.UtcNow });

        // Wine 3 - Decent ratings (should get Bronze)
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 2.0m, Nose = 2.5m, Taste = 9.5m, Comment = "Pent rosé", WineId = wines[2].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 2.1m, Nose = 2.4m, Taste = 9.8m, Comment = "Frisk og lett", WineId = wines[2].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 1.9m, Nose = 2.6m, Taste = 9.6m, Comment = "Grei vin", WineId = wines[2].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 2.0m, Nose = 2.5m, Taste = 9.7m, Comment = "Akseptabel", WineId = wines[2].WineId, RatingDate = DateTime.UtcNow });

        // Wine 4 - Special merit ratings
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 1.8m, Nose = 2.2m, Taste = 8.5m, Comment = "Interessant", WineId = wines[3].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 1.9m, Nose = 2.3m, Taste = 8.8m, Comment = "Lovende", WineId = wines[3].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 1.8m, Nose = 2.1m, Taste = 8.6m, Comment = "God potensial", WineId = wines[3].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 1.9m, Nose = 2.2m, Taste = 8.7m, Comment = "Kan bli bedre", WineId = wines[3].WineId, RatingDate = DateTime.UtcNow });

        // Wine 5 - High spread ratings (should trigger outlier)
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 2.5m, Nose = 3.0m, Taste = 10.0m, Comment = "Meget bra", WineId = wines[4].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 1.5m, Nose = 1.8m, Taste = 6.5m, Comment = "Ikke imponert", WineId = wines[4].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 2.2m, Nose = 2.8m, Taste = 9.5m, Comment = "God vin", WineId = wines[4].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 2.0m, Nose = 2.5m, Taste = 9.0m, Comment = "Ganske bra", WineId = wines[4].WineId, RatingDate = DateTime.UtcNow });

        // Wine 6 - Below gate values (should fail)
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 1.5m, Nose = 1.5m, Taste = 5.0m, Comment = "Under forventet", WineId = wines[5].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 1.6m, Nose = 1.6m, Taste = 5.5m, Comment = "Svak", WineId = wines[5].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 1.4m, Nose = 1.7m, Taste = 5.2m, Comment = "Ikke godkjent", WineId = wines[5].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 1.5m, Nose = 1.5m, Taste = 5.3m, Comment = "Mangler kvalitet", WineId = wines[5].WineId, RatingDate = DateTime.UtcNow });

        // Wine 7 - Nordic guest wine (good ratings)
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans", Appearance = 2.4m, Nose = 3.2m, Taste = 10.5m, Comment = "Nordisk kvalitet", WineId = wines[6].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Appearance = 2.3m, Nose = 3.0m, Taste = 10.3m, Comment = "Fin svensk vin", WineId = wines[6].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans", Appearance = 2.5m, Nose = 3.3m, Taste = 10.7m, Comment = "Meget god", WineId = wines[6].WineId, RatingDate = DateTime.UtcNow });
        await wineRatingRepo.AddWineRatingAsync(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola", Appearance = 2.4m, Nose = 3.1m, Taste = 10.4m, Comment = "Imponerende", WineId = wines[6].WineId, RatingDate = DateTime.UtcNow });
    }
}
