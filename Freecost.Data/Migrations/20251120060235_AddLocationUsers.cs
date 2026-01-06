using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Freecost.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationUsers_Locations_LocationId",
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

            migrationBuilder.CreateIndex(
                name: "IX_LocationUsers_LocationId",
                table: "LocationUsers",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationUsers");

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8411), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8413) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8500), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8500) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8503), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8504) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("14141414-1414-1414-1414-141414141414"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8507), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8508) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("15151515-1515-1515-1515-151515151515"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8510), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8511) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8434), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8435) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8438), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8439) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8442), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8442) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8445), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8445) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8459), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8459) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8462), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8462) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8465), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8465) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8467), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8468) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8472), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8472) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8475), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8475) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8478), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8478) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8480), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8481) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8486), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8486) });

            migrationBuilder.UpdateData(
                table: "Allergens",
                keyColumn: "Id",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                columns: new[] { "CreatedAt", "ModifiedAt" },
                values: new object[] { new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8489), new DateTime(2025, 11, 15, 7, 40, 14, 901, DateTimeKind.Utc).AddTicks(8489) });
        }
    }
}
