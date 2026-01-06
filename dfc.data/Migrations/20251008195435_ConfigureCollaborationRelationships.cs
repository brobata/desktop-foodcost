using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCollaborationRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntreeComments_EntreeComments_ParentCommentId",
                table: "EntreeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_EntreeComments_Users_UserId",
                table: "EntreeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeComments_RecipeComments_ParentCommentId",
                table: "RecipeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeComments_Users_UserId",
                table: "RecipeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_SharedRecipes_Users_SharedByUserId",
                table: "SharedRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_SharedRecipes_Users_SharedWithUserId",
                table: "SharedRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SharedRecipes_RecipeId",
                table: "SharedRecipes");

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

            migrationBuilder.CreateIndex(
                name: "IX_SharedRecipes_RecipeId_SharedWithUserId",
                table: "SharedRecipes",
                columns: new[] { "RecipeId", "SharedWithUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntreeComments_EntreeComments_ParentCommentId",
                table: "EntreeComments",
                column: "ParentCommentId",
                principalTable: "EntreeComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntreeComments_Users_UserId",
                table: "EntreeComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeComments_RecipeComments_ParentCommentId",
                table: "RecipeComments",
                column: "ParentCommentId",
                principalTable: "RecipeComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeComments_Users_UserId",
                table: "RecipeComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SharedRecipes_Users_SharedByUserId",
                table: "SharedRecipes",
                column: "SharedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SharedRecipes_Users_SharedWithUserId",
                table: "SharedRecipes",
                column: "SharedWithUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntreeComments_EntreeComments_ParentCommentId",
                table: "EntreeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_EntreeComments_Users_UserId",
                table: "EntreeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeComments_RecipeComments_ParentCommentId",
                table: "RecipeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeComments_Users_UserId",
                table: "RecipeComments");

            migrationBuilder.DropForeignKey(
                name: "FK_SharedRecipes_Users_SharedByUserId",
                table: "SharedRecipes");

            migrationBuilder.DropForeignKey(
                name: "FK_SharedRecipes_Users_SharedWithUserId",
                table: "SharedRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SharedRecipes_RecipeId_SharedWithUserId",
                table: "SharedRecipes");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1655), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1657) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1717), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1717) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1719), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1719) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1722), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1722) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1724), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1724) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1671), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1671) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1673), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1674) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1685), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1685) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1687), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1688) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1691), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1691) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1693), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1694) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1695), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1696) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1698), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1698) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1701), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1701) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1704), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1704) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1708), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1708) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1711), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1711) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1713), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1713) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1715), new DateTime(2025, 10, 8, 19, 44, 16, 22, DateTimeKind.Utc).AddTicks(1715) });

            migrationBuilder.CreateIndex(
                name: "IX_SharedRecipes_RecipeId",
                table: "SharedRecipes",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntreeComments_EntreeComments_ParentCommentId",
                table: "EntreeComments",
                column: "ParentCommentId",
                principalTable: "EntreeComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EntreeComments_Users_UserId",
                table: "EntreeComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeComments_RecipeComments_ParentCommentId",
                table: "RecipeComments",
                column: "ParentCommentId",
                principalTable: "RecipeComments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeComments_Users_UserId",
                table: "RecipeComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SharedRecipes_Users_SharedByUserId",
                table: "SharedRecipes",
                column: "SharedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SharedRecipes_Users_SharedWithUserId",
                table: "SharedRecipes",
                column: "SharedWithUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
