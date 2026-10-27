using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClosetAPI.Infrastructure.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Slot = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ImageUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Variant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formality = table.Column<int>(type: "int", nullable: false),
                    Fabric = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TimesWorn = table.Column<int>(type: "int", nullable: false),
                    LastWorn = table.Column<DateOnly>(type: "date", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RainOk = table.Column<bool>(type: "bit", nullable: true),
                    NoRain = table.Column<bool>(type: "bit", nullable: true),
                    Seasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Outfits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Saved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Pieces_Acc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pieces_Bottom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pieces_Outer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pieces_Shoes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pieces_Top = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outfits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TryOnRenders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OutfitId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PiecesKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonPhotoFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Placeholder = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TryOnRenders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Onboarded = table.Column<bool>(type: "bit", nullable: false),
                    PersonPhotoFile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TodayOutfitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TodayOutfitId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StyleVariation = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wears",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    OutfitId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wears", x => x.Date);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Outfits");

            migrationBuilder.DropTable(
                name: "TryOnRenders");

            migrationBuilder.DropTable(
                name: "UserStates");

            migrationBuilder.DropTable(
                name: "Wears");
        }
    }
}
