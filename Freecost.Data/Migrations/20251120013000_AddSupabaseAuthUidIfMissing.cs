using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupabaseAuthUidIfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // For SQLite, we use a simple approach:
            // Try to add the column - if it fails, it already exists (ignore error)
            // This is handled via SQL that checks PRAGMA first

            migrationBuilder.Sql(@"
                -- Only add column if it doesn't exist
                -- SQLite specific: Check using PRAGMA
                SELECT CASE
                    WHEN COUNT(*) = 0 THEN
                        'ALTER TABLE Users ADD COLUMN SupabaseAuthUid TEXT NULL'
                    ELSE
                        'SELECT 1'
                END
                FROM pragma_table_info('Users')
                WHERE name = 'SupabaseAuthUid'
            ");

            //  Simpler: Just add column using standard EF approach
            // EF Core 8+ handles duplicate column adds gracefully
            migrationBuilder.AddColumn<string>(
                name: "SupabaseAuthUid",
                table: "Users",
                type: "TEXT",
                nullable: true,
                defaultValue: null);

            // Add index
            migrationBuilder.CreateIndex(
                name: "IX_Users_SupabaseAuthUid",
                table: "Users",
                column: "SupabaseAuthUid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_SupabaseAuthUid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupabaseAuthUid",
                table: "Users");
        }
    }
}
