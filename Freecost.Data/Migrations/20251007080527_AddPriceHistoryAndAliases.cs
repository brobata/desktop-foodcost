using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceHistoryAndAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4135), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4136) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4196), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4196) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4198), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4198) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4201), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4201) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4203), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4203) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4149), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4150) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4153), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4153) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4161), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4161) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4163), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4164) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4167), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4167) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4169), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4169) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4171), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4171) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4177), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4178) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4180), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4180) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4183), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4183) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4187), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4188) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4190), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4190) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4192), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4192) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4194), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4194) });
        }
    }
}
