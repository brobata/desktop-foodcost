using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniversalConversionFieldsToIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoConversionEnabled",
                table: "Ingredients",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConversionLastUpdated",
                table: "Ingredients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConversionSource",
                table: "Ingredients",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DensityProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Keywords = table.Column<string>(type: "TEXT", nullable: false),
                    GramsPerCup = table.Column<decimal>(type: "TEXT", nullable: true),
                    GramsPerTablespoon = table.Column<decimal>(type: "TEXT", nullable: true),
                    GramsPerTeaspoon = table.Column<decimal>(type: "TEXT", nullable: true),
                    GramsPerFluidOunce = table.Column<decimal>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    UsdaFdcId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DensityProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngredientConversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FromQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    FromUnit = table.Column<int>(type: "INTEGER", nullable: false),
                    ToQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    ToUnit = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    UsdaFdcId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientConversions_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IngredientConversions_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8411), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8413) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8500), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8500) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8503), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8504) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8507), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8508) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8510), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8511) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8434), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8435) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8438), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8439) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8442), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8442) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8445), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8445) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8459), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8459) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8462), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8462) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8465), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8465) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8467), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8468) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8472), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8472) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8475), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8475) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8478), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8478) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8480), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8481) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8486), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8486) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8489), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8489) });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientConversions_IngredientId",
                table: "IngredientConversions",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientConversions_LocationId",
                table: "IngredientConversions",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DensityProfiles");

            migrationBuilder.DropTable(
                name: "IngredientConversions");

            migrationBuilder.DropColumn(
                name: "AutoConversionEnabled",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "ConversionLastUpdated",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "ConversionSource",
                table: "Ingredients");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2489), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2490) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2551), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2551) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2553), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2554) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2556), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2556) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2558), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2558) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2504), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2504) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2508), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2508) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2510), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2511) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2512), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2513) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2516), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2516) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2518), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2518) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2531), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2531) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2533), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2534) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2536), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2536) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2538), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2539) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2541), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2541) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2543), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2543) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2545), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2545) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2547), new DateTime(2025, 11, 11, 19, 54, 33, 726, DateTimeKind.Utc).AddTicks(2547) });
        }
    }
}
