using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixRecipeCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntreeRecipes_Recipes_RecipeId",
                table: "EntreeRecipes");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1655), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1657) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1717), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1717) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1719), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1719) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1722), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1722) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1724), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1724) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1671), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1671) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1673), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1674) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1685), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1685) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1687), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1688) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1691), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1691) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1693), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1694) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1695), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1696) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1698), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1698) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1701), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1701) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1704), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1704) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1708), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1708) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1711), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1711) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1713), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1713) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1715), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1715) });

            migrationBuilder.AddForeignKey(
                name: "FK_EntreeRecipes_Recipes_RecipeId",
                table: "EntreeRecipes",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntreeRecipes_Recipes_RecipeId",
                table: "EntreeRecipes");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2276), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2278) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2336), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2336) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2339), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2340) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2342), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2342) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2344), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2345) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2293), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2293) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2296), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2296) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2299), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2299) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2301), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2301) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2311), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2311) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2313), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2313) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2315), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2316) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2319), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2320) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2322), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2322) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2325), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2325) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2327), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2327) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2329), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2330) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2331), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2332) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2333), new DateTime(2025, 10, 8, 19, 30, 48, 626, DateTimeKind.Utc).AddTicks(2334) });

            migrationBuilder.AddForeignKey(
                name: "FK_EntreeRecipes_Recipes_RecipeId",
                table: "EntreeRecipes",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
