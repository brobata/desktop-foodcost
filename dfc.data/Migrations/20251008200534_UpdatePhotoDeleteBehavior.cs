using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhotoDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Entrees_EntreeId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Recipes_RecipeId",
                table: "Photos");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Entrees_EntreeId",
                table: "Photos",
                column: "EntreeId",
                principalTable: "Entrees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Recipes_RecipeId",
                table: "Photos",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Entrees_EntreeId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Recipes_RecipeId",
                table: "Photos");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6173), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6173) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6232), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6232) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6234), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6234) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6237), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6237) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6238), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6239) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6187), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6187) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6190), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6190) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6192), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6192) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6194), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6195) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6198), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6199) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6200), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6201) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6211), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6211) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6213), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6213) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6216), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6216) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6218), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6218) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6221), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6221) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6223), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6223) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6225), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6225) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6227), new DateTime(2025, 10, 8, 19, 54, 34, 929, DateTimeKind.Utc).AddTicks(6227) });

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Entrees_EntreeId",
                table: "Photos",
                column: "EntreeId",
                principalTable: "Entrees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Recipes_RecipeId",
                table: "Photos",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }
    }
}
