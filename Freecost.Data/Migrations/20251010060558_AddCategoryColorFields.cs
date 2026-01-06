using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryColorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrintSettingsJson",
                table: "UserPreferences",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryColor",
                table: "Recipes",
                type: "TEXT",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryColor",
                table: "Ingredients",
                type: "TEXT",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryColor",
                table: "Entrees",
                type: "TEXT",
                maxLength: 7,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2504), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2507) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2566), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2566) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2568), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2568) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2577), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2577) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2579), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2580) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2521), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2521) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2524), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2525) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2527), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2527) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2529), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2529) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2533), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2533) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2544), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2544) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2546), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2546) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2548), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2548) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2551), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2551) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2553), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2554) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2555), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2556) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2557), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2558) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2560), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2560) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2564), new DateTime(2025, 10, 10, 6, 5, 57, 774, DateTimeKind.Utc).AddTicks(2564) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrintSettingsJson",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "CategoryColor",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CategoryColor",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CategoryColor",
                table: "Entrees");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9279), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9281) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9343), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9343) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9346), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9346) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9358), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9358) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9360), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9360) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9296), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9296) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9300), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9301) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9303), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9303) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9305), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9305) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9309), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9309) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9311), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9311) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9322), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9322) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9325), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9325) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9328), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9328) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9331), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9331) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9333), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9333) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9335), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9335) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9337), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9337) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9339), new DateTime(2025, 10, 9, 18, 39, 25, 497, DateTimeKind.Utc).AddTicks(9339) });
        }
    }
}
