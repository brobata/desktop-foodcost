using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToRecipeAndEntree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Recipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Entrees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6117), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6119) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6179), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6180) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6181), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6182) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6184), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6184) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6186), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6186) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6133), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6133) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6135), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6136) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6148), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6149) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6151), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6151) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6155), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6155) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6157), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6157) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6159), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6159) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6161), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6162) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6164), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6164) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6166), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6167) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6171), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6171) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6173), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6173) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6175), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6175) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6177), new DateTime(2025, 10, 7, 14, 24, 9, 719, DateTimeKind.Utc).AddTicks(6178) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Entrees");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1624), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1625) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1687), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1688) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1689), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1690) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1692), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1693) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1694), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1695) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1641), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1641) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1644), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1644) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1655), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1655) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1657), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1658) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1661), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1661) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1663), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1664) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1666), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1666) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1668), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1668) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1671), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1671) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1674), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1674) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1678), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1679) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1681), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1681) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1683), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1684) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1685), new DateTime(2025, 10, 7, 8, 5, 27, 247, DateTimeKind.Utc).AddTicks(1686) });
        }
    }
}
