using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparationInstructionsToEntree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreparationInstructions",
                table: "Entrees",
                type: "TEXT",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreparationInstructions",
                table: "Entrees");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3039), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3041) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3126), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3126) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3131), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3132) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3135), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3136) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3138), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3139) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3063), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3063) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3069), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3069) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3073), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3074) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3077), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3077) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3082), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3083) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3086), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3086) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3089), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3090) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3095), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3095) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3107), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3107) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3110), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3111) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3113), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3114) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3116), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3117) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3119), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3120) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3122), new DateTime(2025, 11, 20, 6, 2, 35, 352, DateTimeKind.Utc).AddTicks(3122) });
        }
    }
}
