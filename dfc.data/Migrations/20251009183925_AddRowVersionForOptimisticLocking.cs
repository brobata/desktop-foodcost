using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionForOptimisticLocking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WasteRecords",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserPreferences",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TeamNotifications",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "TeamActivityFeeds",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SharedRecipes",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Recipes",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RecipeComments",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "RecipeAllergens",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PriceHistories",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Photos",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Locations",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Ingredients",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "IngredientAllergens",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Entrees",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntreeRecipes",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntreeIngredients",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntreeComments",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "EntreeAllergens",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DraftItems",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DeletedItems",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ChangeHistories",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AuditLogs",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApprovalWorkflows",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ApprovalComments",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Allergens",
                type: "BLOB",
                rowVersion: true,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TeamNotifications");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "TeamActivityFeeds");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SharedRecipes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RecipeComments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "RecipeAllergens");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PriceHistories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "IngredientAllergens");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Entrees");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntreeRecipes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntreeIngredients");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntreeComments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "EntreeAllergens");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DraftItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DeletedItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ChangeHistories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApprovalWorkflows");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ApprovalComments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Allergens");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7045), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7047) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7101), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7101) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7105), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7105) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7108), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7108) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7110), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7110) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7063), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7064) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7067), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7067) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7069), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7070) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7072), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7073) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7076), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7076) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7078), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7079) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7081), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7081) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7085), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7085) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7088), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7088) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7090), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7090) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7092), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7093) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7094), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7095) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7096), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7097) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7099), new DateTime(2025, 10, 9, 15, 16, 8, 411, DateTimeKind.Utc).AddTicks(7099) });
        }
    }
}
