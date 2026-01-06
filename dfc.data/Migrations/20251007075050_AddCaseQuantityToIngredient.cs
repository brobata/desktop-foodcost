using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dfc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseQuantityToIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entrees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MenuPrice = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PlatingEquipment = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entrees_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    CaseQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    VendorName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    VendorSku = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UseAlternateUnit = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlternateUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    AlternateConversionQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    AlternateConversionUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Instructions = table.Column<string>(type: "TEXT", nullable: true),
                    Yield = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    YieldUnit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrepTimeMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhotoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SupabaseAuthUid = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntreeAllergens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntreeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AllergenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsAutoDetected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourceIngredients = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntreeAllergens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntreeAllergens_Allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "Allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntreeAllergens_Entrees_EntreeId",
                        column: x => x.EntreeId,
                        principalTable: "Entrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntreeIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntreeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntreeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntreeIngredients_Entrees_EntreeId",
                        column: x => x.EntreeId,
                        principalTable: "Entrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntreeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngredientAliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AliasName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientAliases_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngredientAllergens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AllergenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsAutoDetected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourceIngredients = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientAllergens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientAllergens_Allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "Allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientAllergens_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAggregated = table.Column<bool>(type: "INTEGER", nullable: false),
                    AggregationType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntreeRecipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntreeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntreeRecipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntreeRecipes_Entrees_EntreeId",
                        column: x => x.EntreeId,
                        principalTable: "Entrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntreeRecipes_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeAllergens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AllergenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsAutoDetected = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourceIngredients = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeAllergens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeAllergens_Allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "Allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeAllergens_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsOptional = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Allergens",
                columns: new[] { "Id", "CreatedAt", "IconPath", "ModifiedAt", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4135), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4136), "Milk", 0 },
                    { new Guid("12121212-1212-1212-1212-121212121212"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4196), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4196), "Contains Alcohol", 15 },
                    { new Guid("13131313-1313-1313-1313-131313131313"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4198), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4198), "Nightshades", 16 },
                    { new Guid("14141414-1414-1414-1414-141414141414"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4201), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4201), "Sulfites", 17 },
                    { new Guid("15151515-1515-1515-1515-151515151515"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4203), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4203), "Added Sugar", 18 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4149), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4150), "Eggs", 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4153), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4153), "Fish", 2 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4161), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4161), "Shellfish", 3 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4163), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4164), "Tree Nuts", 4 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4167), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4167), "Peanuts", 5 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4169), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4169), "Wheat", 6 },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4171), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4171), "Soybeans", 7 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4177), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4178), "Sesame", 8 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4180), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4180), "Vegan", 9 },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4183), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4183), "Vegetarian", 10 },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4187), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4188), "Pescatarian", 11 },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4190), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4190), "Gluten Free", 12 },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4192), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4192), "Kosher", 13 },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4194), null, new DateTime(2025, 10, 7, 7, 50, 50, 351, DateTimeKind.Utc).AddTicks(4194), "Halal", 14 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Allergens_Type",
                table: "Allergens",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntreeAllergens_AllergenId",
                table: "EntreeAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_EntreeAllergens_EntreeId_AllergenId",
                table: "EntreeAllergens",
                columns: new[] { "EntreeId", "AllergenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntreeIngredients_EntreeId",
                table: "EntreeIngredients",
                column: "EntreeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntreeIngredients_IngredientId",
                table: "EntreeIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_EntreeRecipes_EntreeId",
                table: "EntreeRecipes",
                column: "EntreeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntreeRecipes_RecipeId",
                table: "EntreeRecipes",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrees_LocationId",
                table: "Entrees",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrees_Name",
                table: "Entrees",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientAliases_AliasName",
                table: "IngredientAliases",
                column: "AliasName");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientAliases_IngredientId",
                table: "IngredientAliases",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientAllergens_AllergenId",
                table: "IngredientAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientAllergens_IngredientId_AllergenId",
                table: "IngredientAllergens",
                columns: new[] { "IngredientId", "AllergenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_LocationId_Name",
                table: "Ingredients",
                columns: new[] { "LocationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Name",
                table: "Ingredients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_IngredientId_RecordedDate",
                table: "PriceHistories",
                columns: new[] { "IngredientId", "RecordedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeAllergens_AllergenId",
                table: "RecipeAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeAllergens_RecipeId_AllergenId",
                table: "RecipeAllergens",
                columns: new[] { "RecipeId", "AllergenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_IngredientId",
                table: "RecipeIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_LocationId_Name",
                table: "Recipes",
                columns: new[] { "LocationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_Name",
                table: "Recipes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupabaseAuthUid",
                table: "Users",
                column: "SupabaseAuthUid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LocationId",
                table: "Users",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntreeAllergens");

            migrationBuilder.DropTable(
                name: "EntreeIngredients");

            migrationBuilder.DropTable(
                name: "EntreeRecipes");

            migrationBuilder.DropTable(
                name: "IngredientAliases");

            migrationBuilder.DropTable(
                name: "IngredientAllergens");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "RecipeAllergens");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Entrees");

            migrationBuilder.DropTable(
                name: "Allergens");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
