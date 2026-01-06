using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeRecipeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeRecipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentRecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComponentRecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeRecipes_Recipes_ComponentRecipeId",
                        column: x => x.ComponentRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeRecipes_Recipes_ParentRecipeId",
                        column: x => x.ParentRecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5202), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5205) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5262), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5262) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5264), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5265) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5267), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5268) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5269), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5270) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5218), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5218) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5222), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5222) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5231), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5232) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5234), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5234) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5238), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5238) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5240), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5240) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5242), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5242) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5244), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5245) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5247), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5247) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5249), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5249) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5253), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5254) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5256), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5256) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5258), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5258) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5260), new DateTime(2025, 11, 26, 6, 15, 21, 872, DateTimeKind.Utc).AddTicks(5260) });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeRecipes_ComponentRecipeId",
                table: "RecipeRecipes",
                column: "ComponentRecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeRecipes_ParentRecipeId",
                table: "RecipeRecipes",
                column: "ParentRecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeRecipes");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7504), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7505) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7570), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7570) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7572), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7572) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7575), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7575) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7577), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7577) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7518), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7518) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7521), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7522) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7524), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7524) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7526), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7526) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7538), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7538) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7540), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7540) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7542), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7542) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7544), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7545) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7547), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7548) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7550), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7550) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7552), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7552) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7554), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7554) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7565), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7565) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7567), new DateTime(2025, 11, 26, 4, 14, 54, 509, DateTimeKind.Utc).AddTicks(7568) });
        }
    }
}
