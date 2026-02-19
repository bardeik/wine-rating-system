using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WineApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WineProducers",
                columns: table => new
                {
                    WineProducerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WineyardName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganisationNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ResponsibleProducerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WineProducers", x => x.WineProducerId);
                });

            migrationBuilder.CreateTable(
                name: "WineRatings",
                columns: table => new
                {
                    WineRatingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Visuality = table.Column<int>(type: "INTEGER", nullable: false),
                    Nose = table.Column<int>(type: "INTEGER", nullable: false),
                    Taste = table.Column<int>(type: "INTEGER", nullable: false),
                    JudgeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    WineId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WineRatings", x => x.WineRatingId);
                });

            migrationBuilder.CreateTable(
                name: "Wines",
                columns: table => new
                {
                    WineId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RatingName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Group = table.Column<int>(type: "INTEGER", nullable: false),
                    Class = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    WineProducerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wines", x => x.WineId);
                });

            migrationBuilder.InsertData(
                table: "TodoItems",
                columns: new[] { "Id", "CreatedAt", "DueDate", "IsCompleted", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Prepare wine samples for tasting" },
                    { 2, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Review judge assignments" },
                    { 3, new DateTime(2024, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Compile final wine ratings report" }
                });

            migrationBuilder.InsertData(
                table: "WineProducers",
                columns: new[] { "WineProducerId", "Address", "City", "Country", "Email", "OrganisationNumber", "ResponsibleProducerName", "WineyardName", "Zip" },
                values: new object[,]
                {
                    { 1, "Test adresse 21", "Oslo", "Norway", "bestWines@fluffy.com", "111122223333445", "Test Testersen", "Oslo Vest Wines AS", "0125" },
                    { 2, "Test adresse Ny 15", "Grimstad", "Norway", "bestWinesEver@fluffier.com", "111122234567890", "Petter Testeren", "Grimstad Vin og Vann AS", "4525" },
                    { 3, "Agder Alle 21", "Kristiansand", "Norway", "bardeh@gmail.com", "222222223333445", "Bård Eik", "Tech Wine AS", "4631" }
                });

            migrationBuilder.InsertData(
                table: "WineRatings",
                columns: new[] { "WineRatingId", "JudgeId", "Nose", "Taste", "Visuality", "WineId" },
                values: new object[,]
                {
                    { 1, "Hans", 4, 5, 5, 1 },
                    { 2, "Petter", 3, 4, 3, 1 },
                    { 3, "Frans", 5, 4, 6, 1 },
                    { 4, "Ola", 5, 4, 4, 1 }
                });

            migrationBuilder.InsertData(
                table: "Wines",
                columns: new[] { "WineId", "Category", "Class", "Group", "Name", "RatingName", "WineProducerId" },
                values: new object[,]
                {
                    { 1, 3, 1, 0, "Polets røde", "Hemmelig Polets Røde", 1 },
                    { 2, 3, 0, 2, "Polets andre røde", "Hemmelig Andre Polets Røde", 1 },
                    { 3, 3, 0, 1, "Polets røde", "Hemmelig Tredje Polets Røde", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "WineProducers");

            migrationBuilder.DropTable(
                name: "WineRatings");

            migrationBuilder.DropTable(
                name: "Wines");
        }
    }
}
