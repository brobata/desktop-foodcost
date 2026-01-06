using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityNameToLocalModifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "LocalModifications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8503), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8504) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8561), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8561) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8563), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8563) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8567), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8568) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8570), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8570) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8525), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8526) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8529), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8529) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8532), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8532) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8534), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8534) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8537), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8538) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8540), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8540) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8542), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8542) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8544), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8549), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8549) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8551), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8551) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8553), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8553) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8555), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8555) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8557), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8557) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8559), new DateTime(2025, 10, 15, 20, 4, 52, 10, DateTimeKind.Utc).AddTicks(8559) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "LocalModifications");

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
        }
    }
}
