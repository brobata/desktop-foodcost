using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalModificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalModifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModificationType = table.Column<int>(type: "INTEGER", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsSynced = table.Column<bool>(type: "INTEGER", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SyncAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LastSyncError = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalModifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalModifications_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2186), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2188) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2244), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2244) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2248), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2248) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2250), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2251) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2253), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2253) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2206), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2207) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2209), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2210) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2212), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2213) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2214), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2215) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2218), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2218) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2220), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2220) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2223), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2223) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2227), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2228) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2230), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2231) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2233), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2233) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2235), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2235) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2237), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2238) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2239), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2240) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2242), new DateTime(2025, 10, 15, 3, 56, 56, 471, DateTimeKind.Utc).AddTicks(2242) });

            migrationBuilder.CreateIndex(
                name: "IX_LocalModifications_EntityType_EntityId",
                table: "LocalModifications",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalModifications_IsSynced",
                table: "LocalModifications",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_LocalModifications_LocationId",
                table: "LocalModifications",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalModifications_ModifiedAt",
                table: "LocalModifications",
                column: "ModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalModifications");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8027), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8029) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8086), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8086) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8088), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8089) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8091), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8091) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8093), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8093) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8042), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8042) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8045), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8045) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8047), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8048) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8058), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8059) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8062), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8062) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8064), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8064) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8066), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8066) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8068), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8069) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8071), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8071) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8073), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8074) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8076), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8076) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8080), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8080) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8082), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8082) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8084), new DateTime(2025, 10, 10, 18, 14, 33, 981, DateTimeKind.Utc).AddTicks(8084) });
        }
    }
}
