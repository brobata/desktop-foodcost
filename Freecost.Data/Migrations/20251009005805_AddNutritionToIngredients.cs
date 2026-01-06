using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionToIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CaloriesPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CarbohydratesPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FatPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FiberPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProteinPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SodiumPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SugarPerUnit",
                table: "Ingredients",
                type: "TEXT",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6360), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6362) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6432), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6432) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6434), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6434) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6437), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6437) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6440), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6441) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6379), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6380) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6394), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6395) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6397), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6397) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6400), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6400) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6403), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6404) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6405), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6406) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6407), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6408) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6410), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6410) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6413), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6413) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6418), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6418) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6420), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6421) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6423), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6423) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6427), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6427) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6430), new DateTime(2025, 10, 9, 0, 58, 4, 866, DateTimeKind.Utc).AddTicks(6430) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloriesPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "CarbohydratesPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "FatPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "FiberPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "ProteinPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "SodiumPerUnit",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "SugarPerUnit",
                table: "Ingredients");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2879), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2881) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2943), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2943) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2945), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2945) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2948), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2948) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2950), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2950) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2894), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2895) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2897), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2897) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2905), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2905) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2908), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2908) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2919), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2919) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2921), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2921) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2923), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2923) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2925), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2926) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2928), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2928) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2930), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2930) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2933), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2933) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2935), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2935) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2939), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2939) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2941), new DateTime(2025, 10, 8, 20, 5, 34, 122, DateTimeKind.Utc).AddTicks(2941) });
        }
    }
}
