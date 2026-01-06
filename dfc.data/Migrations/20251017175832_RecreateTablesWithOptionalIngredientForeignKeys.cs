using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecreateTablesWithOptionalIngredientForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite doesn't support altering foreign keys directly
            // We need to recreate the RecipeIngredients table with optional IngredientId foreign key

            // Step 1: Disable foreign key constraints
            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;");

            // Step 2: Rename old table
            migrationBuilder.RenameTable(
                name: "RecipeIngredients",
                newName: "RecipeIngredients_old");

            // Step 3: Create new table with optional foreign key
            migrationBuilder.Sql(@"
                CREATE TABLE RecipeIngredients (
                    Id TEXT NOT NULL PRIMARY KEY,
                    RecipeId TEXT NOT NULL,
                    IngredientId TEXT,
                    Quantity TEXT NOT NULL,
                    Unit INTEGER NOT NULL,
                    DisplayText TEXT,
                    UnmatchedIngredientName TEXT,
                    IsOptional INTEGER NOT NULL,
                    SortOrder INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ModifiedAt TEXT NOT NULL,
                    FOREIGN KEY (RecipeId) REFERENCES Recipes(Id) ON DELETE CASCADE,
                    FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id) ON DELETE CASCADE
                );
            ");

            // Step 4: Copy data from old table to new
            migrationBuilder.Sql(@"
                INSERT INTO RecipeIngredients (Id, RecipeId, IngredientId, Quantity, Unit, DisplayText, UnmatchedIngredientName, IsOptional, SortOrder, CreatedAt, ModifiedAt)
                SELECT Id, RecipeId, IngredientId, Quantity, Unit, DisplayText, UnmatchedIngredientName, IsOptional, SortOrder, CreatedAt, ModifiedAt
                FROM RecipeIngredients_old;
            ");

            // Step 5: Drop old table
            migrationBuilder.DropTable("RecipeIngredients_old");

            // Step 6: Re-enable foreign key constraints
            migrationBuilder.Sql("PRAGMA foreign_keys = ON;");

            // Update allergen timestamps
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the migration: recreate RecipeIngredients with REQUIRED foreign key

            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;");

            migrationBuilder.RenameTable(
                name: "RecipeIngredients",
                newName: "RecipeIngredients_new");

            migrationBuilder.Sql(@"
                CREATE TABLE RecipeIngredients (
                    Id TEXT NOT NULL PRIMARY KEY,
                    RecipeId TEXT NOT NULL,
                    IngredientId TEXT NOT NULL,
                    Quantity TEXT NOT NULL,
                    Unit INTEGER NOT NULL,
                    DisplayText TEXT,
                    UnmatchedIngredientName TEXT,
                    IsOptional INTEGER NOT NULL,
                    SortOrder INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ModifiedAt TEXT NOT NULL,
                    FOREIGN KEY (RecipeId) REFERENCES Recipes(Id) ON DELETE CASCADE,
                    FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id) ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO RecipeIngredients (Id, RecipeId, IngredientId, Quantity, Unit, DisplayText, UnmatchedIngredientName, IsOptional, SortOrder, CreatedAt, ModifiedAt)
                SELECT Id, RecipeId, IngredientId, Quantity, Unit, DisplayText, UnmatchedIngredientName, IsOptional, SortOrder, CreatedAt, ModifiedAt
                FROM RecipeIngredients_new
                WHERE IngredientId IS NOT NULL;
            ");

            migrationBuilder.DropTable("RecipeIngredients_new");

            migrationBuilder.Sql("PRAGMA foreign_keys = ON;");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8445), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8447) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8533), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8533) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8536), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8536) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8541), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8541) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8543), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8468), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8468) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8471), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8472) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8483), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8484) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8494), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8495) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8501), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8501) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8503), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8504) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8506), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8507) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8509), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8509) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8513), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8513) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8516), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8516) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8519), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8519) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8524), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8525) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8527), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8528) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8530), new DateTime(2025, 10, 17, 16, 7, 1, 486, DateTimeKind.Utc).AddTicks(8531) });
        }
    }
}
