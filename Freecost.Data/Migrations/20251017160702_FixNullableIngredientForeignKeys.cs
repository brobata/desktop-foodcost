using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixNullableIngredientForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientMatchMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImportName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MatchedIngredientId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MatchedRecipeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientMatchMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientMatchMappings_Ingredients_MatchedIngredientId",
                        column: x => x.MatchedIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IngredientMatchMappings_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientMatchMappings_Recipes_MatchedRecipeId",
                        column: x => x.MatchedRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8445), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8447) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8533), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8533) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8536), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8536) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8541), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8541) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8543), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8468), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8468) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8471), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8472) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8483), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8484) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8494), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8495) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8501), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8501) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8503), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8504) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8506), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8507) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8509), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8509) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8513), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8513) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8516), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8516) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8519), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8519) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8524), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8525) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8527), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8528) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8530), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8531) });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientMatchMappings_LocationId",
                table: "IngredientMatchMappings",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientMatchMappings_MatchedIngredientId",
                table: "IngredientMatchMappings",
                column: "MatchedIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientMatchMappings_MatchedRecipeId",
                table: "IngredientMatchMappings",
                column: "MatchedRecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientMatchMappings");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5200), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5201) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5261), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5261) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5263), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5264) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5271), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5271) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5273), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5273) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5215), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5216) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5219), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5219) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5221), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5222) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5224), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5224) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5227), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5227) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5230), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5230) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5240), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5240) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5243), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5243) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5246), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5246) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5248), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5248) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5250), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5250) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5252), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5252) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5254), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5254) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5256), new DateTime(2025, 10, 16, 12, 23, 52, 343, DateTimeKind.Utc).AddTicks(5256) });
        }
    }
}
