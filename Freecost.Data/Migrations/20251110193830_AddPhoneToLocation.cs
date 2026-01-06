using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Locations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5213), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5215) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5302), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5302) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5304), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5304) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5307), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5307) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5309), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5309) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5243), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5243) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5246), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5247) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5257), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5257) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5259), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5259) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5263), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5263) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5271), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5271) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5273), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5273) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5275), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5276) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5278), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5278) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5280), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5281) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5286), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5286) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5288), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5288) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5291), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5291) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5293), new DateTime(2025, 11, 10, 19, 38, 29, 661, DateTimeKind.Utc).AddTicks(5300) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Locations");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5420), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5422) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5485), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5486) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5488), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5488) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5491), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5491) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5493), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5494) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5436), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5436) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5438), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5439) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5441), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5441) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5444), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5444) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5459), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5459) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5462), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5462) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5464), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5464) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5466), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5466) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5469), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5469) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5472), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5472) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5474), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5474) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5476), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5476) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5480), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5481) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5483), new DateTime(2025, 10, 17, 17, 58, 31, 982, DateTimeKind.Utc).AddTicks(5483) });
        }
    }
}
