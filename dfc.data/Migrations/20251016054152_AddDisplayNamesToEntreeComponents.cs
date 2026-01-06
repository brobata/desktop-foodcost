using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNamesToEntreeComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "EntreeRecipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "EntreeIngredients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 962, DateTimeKind.Utc).AddTicks(9991), new DateTime(2025, 10, 16, 5, 41, 51, 962, DateTimeKind.Utc).AddTicks(9994) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(54), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(54) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(56), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(56) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(59), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(59) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(61), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(61) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(8), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(9) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(11), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(12) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(14), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(14) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(16), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(17) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(20), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(20) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(31), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(31) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(33), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(33) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(35), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(36) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(38), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(38) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(40), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(40) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(42), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(43) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(44), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(45) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(46), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(47) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(51), new DateTime(2025, 10, 16, 5, 41, 51, 963, DateTimeKind.Utc).AddTicks(51) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "EntreeRecipes");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "EntreeIngredients");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7663), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7664) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7721), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7721) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7725), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7725) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7728), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7728) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7729), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7730) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7677), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7678) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7681), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7681) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7684), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7684) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7693), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7693) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7696), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7697) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7698), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7699) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7700), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7701) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7704), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7705) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7707), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7708) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7710), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7711) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7713), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7713) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7715), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7715) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7717), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7717) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7719), new DateTime(2025, 10, 15, 21, 17, 17, 568, DateTimeKind.Utc).AddTicks(7719) });
        }
    }
}
