using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeVersionControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ChangeDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true),
                    Yield = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    YieldUnit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PrepTimeMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DietaryLabels = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Calories = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Protein = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Carbs = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Fat = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Fiber = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Sugar = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Sodium = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    IngredientsJson = table.Column<string>(type: "TEXT", nullable: true),
                    AllergensJson = table.Column<string>(type: "TEXT", nullable: true),
                    TotalCost = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeVersions_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2557), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2560) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2641), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2641) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2643), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2644) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2647), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2647) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2652), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2652) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2578), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2578) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2595), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2595) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2599), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2599) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2602), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2602) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2607), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2607) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2610), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2610) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2613), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2613) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2616), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2616) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2621), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2621) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2627), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2627) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2630), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2630) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2633), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2633) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2635), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2636) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2638), new DateTime(2025, 10, 15, 7, 16, 1, 772, DateTimeKind.Utc).AddTicks(2638) });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersions_CreatedAt",
                table: "RecipeVersions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersions_RecipeId",
                table: "RecipeVersions",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersions_RecipeId_VersionNumber",
                table: "RecipeVersions",
                columns: new[] { "RecipeId", "VersionNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeVersions");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2186), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2188) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2244), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2244) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2248), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2248) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2250), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2251) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2253), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2253) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2206), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2207) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2209), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2210) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2212), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2213) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2214), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2215) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2218), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2218) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2220), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2220) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2223), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2223) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2227), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2228) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2230), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2231) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2233), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2233) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2235), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2235) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2237), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2238) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2239), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2240) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2242), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2242) });
        }
    }
}
