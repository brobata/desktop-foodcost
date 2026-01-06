// Location: Dfc.Data/LocalDatabase/DfcDbContext.cs
// Action: REPLACE entire file

using Microsoft.EntityFrameworkCore;
using Dfc.Core.Models;
using Dfc.Core.Enums;
using System.Collections.Generic;
using System;

namespace Dfc.Data.LocalDatabase;

public class DfcDbContext : DbContext
{
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
    public DbSet<IngredientAlias> IngredientAliases { get; set; } = null!;
    public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
    public DbSet<RecipeRecipe> RecipeRecipes { get; set; } = null!;
    public DbSet<Entree> Entrees { get; set; } = null!;
    public DbSet<EntreeRecipe> EntreeRecipes { get; set; } = null!;
    public DbSet<EntreeIngredient> EntreeIngredients { get; set; } = null!;
    public DbSet<Allergen> Allergens { get; set; } = null!;
    public DbSet<RecipeAllergen> RecipeAllergens { get; set; } = null!;
    public DbSet<EntreeAllergen> EntreeAllergens { get; set; } = null!;
    public DbSet<IngredientAllergen> IngredientAllergens { get; set; } = null!;
    public DbSet<WasteRecord> WasteRecords { get; set; } = null!;
    public DbSet<DeletedItem> DeletedItems { get; set; } = null!;
    public DbSet<Photo> Photos { get; set; } = null!;
    public DbSet<DraftItem> DraftItems { get; set; } = null!;
    public DbSet<RecipeVersion> RecipeVersions { get; set; } = null!;
    public DbSet<IngredientMatchMapping> IngredientMatchMappings { get; set; } = null!;

    public DbSet<LocationUser> LocationUsers { get; set; } = null!;

    // Universal Conversion System
    public DbSet<IngredientConversion> IngredientConversions { get; set; } = null!;
    public DbSet<DensityProfile> DensityProfiles { get; set; } = null!;

    // Delta Sync Optimization
    public DbSet<LocalModification> LocalModifications { get; set; } = null!;

    // v1.5.0 - Multi-User & Collaboration
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<UserPreferences> UserPreferences { get; set; } = null!;
    public DbSet<SharedRecipe> SharedRecipes { get; set; } = null!;
    public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; } = null!;
    public DbSet<ApprovalComment> ApprovalComments { get; set; } = null!;
    public DbSet<RecipeComment> RecipeComments { get; set; } = null!;
    public DbSet<EntreeComment> EntreeComments { get; set; } = null!;
    public DbSet<ChangeHistory> ChangeHistories { get; set; } = null!;
    public DbSet<TeamNotification> TeamNotifications { get; set; } = null!;
    public DbSet<TeamActivityFeed> TeamActivityFeeds { get; set; } = null!;

    public DfcDbContext(DbContextOptions<DfcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Location
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.UserId).HasMaxLength(128); // Supabase Auth UID - NULL for offline locations
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.UserId);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SupabaseAuthUid).HasMaxLength(128);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.SupabaseAuthUid);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Users)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Ingredient
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CategoryColor).HasMaxLength(7); // Hex color like "#E91E63"
            entity.Property(e => e.VendorName).HasMaxLength(200);
            entity.Property(e => e.VendorSku).HasMaxLength(100);
            entity.Property(e => e.CurrentPrice).HasPrecision(10, 4);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.LocationId, e.Name });

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Ingredients)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Nutrition fields
            entity.Property(e => e.CaloriesPerUnit).HasPrecision(10, 2);
            entity.Property(e => e.ProteinPerUnit).HasPrecision(10, 2);
            entity.Property(e => e.CarbohydratesPerUnit).HasPrecision(10, 2);
            entity.Property(e => e.FatPerUnit).HasPrecision(10, 2);
            entity.Property(e => e.FiberPerUnit).HasPrecision(10, 2);
            entity.Property(e => e.SugarPerUnit).HasPrecision(10, 2);
            entity.Property(e => e.SodiumPerUnit).HasPrecision(10, 2);
        });

        // IngredientAlias
        modelBuilder.Entity<IngredientAlias>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AliasName).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.AliasName);

            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.Aliases)
                .HasForeignKey(e => e.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PriceHistory
        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(10, 4);
            entity.HasIndex(e => new { e.IngredientId, e.RecordedDate });

            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.PriceHistory)
                .HasForeignKey(e => e.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Recipe
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.CalculatedNutrition);
            entity.Ignore(e => e.NutritionPerServing);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CategoryColor).HasMaxLength(7); // Hex color like "#E91E63"
            entity.Property(e => e.YieldUnit).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Yield).HasPrecision(10, 2);
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.LocationId, e.Name });
            entity.Ignore(e => e.TotalCost);
            entity.Ignore(e => e.CostPerServing);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Recipes)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RecipeIngredient
        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 4);
            entity.Property(e => e.DisplayText).HasMaxLength(100);
            entity.Ignore(e => e.CalculatedCost);
            entity.Ignore(e => e.HasValidCost);
            entity.Ignore(e => e.CostWarningMessage);

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(e => e.IngredientId)
                .IsRequired(false) // Allow null IngredientId for unmatched ingredients
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RecipeRecipe (recipe-in-recipe relationship)
        modelBuilder.Entity<RecipeRecipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 4);
            entity.Property(e => e.DisplayName).HasMaxLength(200);

            entity.HasOne(e => e.ParentRecipe)
                .WithMany(r => r.RecipeRecipes)
                .HasForeignKey(e => e.ParentRecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ComponentRecipe)
                .WithMany()
                .HasForeignKey(e => e.ComponentRecipeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of component recipe if used
        });

        // RecipeVersion - Version Control
        modelBuilder.Entity<RecipeVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.YieldUnit).HasMaxLength(50);
            entity.Property(e => e.Yield).HasPrecision(10, 2);
            entity.Property(e => e.Difficulty).HasMaxLength(50);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.DietaryLabels).HasMaxLength(500);
            entity.Property(e => e.ChangeDescription).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.TotalCost).HasPrecision(10, 4);
            entity.Property(e => e.Calories).HasPrecision(10, 2);
            entity.Property(e => e.Protein).HasPrecision(10, 2);
            entity.Property(e => e.Carbs).HasPrecision(10, 2);
            entity.Property(e => e.Fat).HasPrecision(10, 2);
            entity.Property(e => e.Fiber).HasPrecision(10, 2);
            entity.Property(e => e.Sugar).HasPrecision(10, 2);
            entity.Property(e => e.Sodium).HasPrecision(10, 2);
            entity.HasIndex(e => e.RecipeId);
            entity.HasIndex(e => new { e.RecipeId, e.VersionNumber });
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Recipe)
                .WithMany()
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Entree
        modelBuilder.Entity<Entree>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CategoryColor).HasMaxLength(7); // Hex color like "#E91E63"
            entity.Property(e => e.MenuPrice).HasPrecision(10, 2);
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
            entity.Property(e => e.PlatingEquipment).HasMaxLength(200); // NEW
            entity.HasIndex(e => e.Name);
            entity.Ignore(e => e.TotalCost);
            entity.Ignore(e => e.FoodCostPercentage);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Entrees)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EntreeRecipe
        modelBuilder.Entity<EntreeRecipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 4);
            entity.Property(e => e.Unit).IsRequired();

            entity.HasOne(e => e.Entree)
                .WithMany(en => en.EntreeRecipes)
                .HasForeignKey(e => e.EntreeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.EntreeRecipes)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EntreeIngredient
        modelBuilder.Entity<EntreeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasPrecision(10, 4);

            entity.HasOne(e => e.Entree)
                .WithMany(en => en.EntreeIngredients)
                .HasForeignKey(e => e.EntreeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.EntreeIngredients)
                .HasForeignKey(e => e.IngredientId)
                .IsRequired(false) // Allow null IngredientId for unmatched ingredients
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Allergen
        modelBuilder.Entity<Allergen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IconPath).HasMaxLength(500);
            entity.HasIndex(e => e.Type).IsUnique();
        });

        // RecipeAllergen
        modelBuilder.Entity<RecipeAllergen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceIngredients).HasMaxLength(1000);
            entity.HasIndex(e => new { e.RecipeId, e.AllergenId }).IsUnique();

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.RecipeAllergens)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Allergen)
                .WithMany(a => a.RecipeAllergens)
                .HasForeignKey(e => e.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EntreeAllergen
        modelBuilder.Entity<EntreeAllergen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceIngredients).HasMaxLength(1000);
            entity.HasIndex(e => new { e.EntreeId, e.AllergenId }).IsUnique();

            entity.HasOne(e => e.Entree)
                .WithMany(en => en.EntreeAllergens)
                .HasForeignKey(e => e.EntreeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Allergen)
                .WithMany(a => a.EntreeAllergens)
                .HasForeignKey(e => e.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IngredientAllergen
        modelBuilder.Entity<IngredientAllergen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceIngredients).HasMaxLength(1000);
            entity.HasIndex(e => new { e.IngredientId, e.AllergenId }).IsUnique();

            entity.HasOne(e => e.Ingredient)
                .WithMany(i => i.IngredientAllergens)
                .HasForeignKey(e => e.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Allergen)
                .WithMany()
                .HasForeignKey(e => e.AllergenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SharedRecipe
        modelBuilder.Entity<SharedRecipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.HasIndex(e => new { e.RecipeId, e.SharedWithUserId });

            entity.HasOne(e => e.Recipe)
                .WithMany()
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SharedByUser)
                .WithMany()
                .HasForeignKey(e => e.SharedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SharedWithUser)
                .WithMany()
                .HasForeignKey(e => e.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RecipeComment
        modelBuilder.Entity<RecipeComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => e.RecipeId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Recipe)
                .WithMany()
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EntreeComment
        modelBuilder.Entity<EntreeComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
            entity.HasIndex(e => e.EntreeId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Entree)
                .WithMany()
                .HasForeignKey(e => e.EntreeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Photo
        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.HasIndex(e => e.RecipeId);
            entity.HasIndex(e => e.EntreeId);

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.Photos)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Entree)
                .WithMany(en => en.Photos)
                .HasForeignKey(e => e.EntreeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeletedItem
        modelBuilder.Entity<DeletedItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SerializedData).IsRequired();
            entity.HasIndex(e => e.LocationId);
            entity.HasIndex(e => e.DeletedDate);

            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LocalModification - Delta Sync Tracking
        modelBuilder.Entity<LocalModification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastSyncError).HasMaxLength(1000);
            entity.HasIndex(e => e.LocationId);
            entity.HasIndex(e => e.IsSynced);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.ModifiedAt);

            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedAllergens(modelBuilder);
    }

    private void SeedAllergens(ModelBuilder modelBuilder)
    {
        var allergens = new List<Allergen>
        {
            // FDA Big 9
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Type = AllergenType.Milk, Name = "Milk" },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Type = AllergenType.Eggs, Name = "Eggs" },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Type = AllergenType.Fish, Name = "Fish" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Type = AllergenType.Shellfish, Name = "Shellfish" },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Type = AllergenType.TreeNuts, Name = "Tree Nuts" },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Type = AllergenType.Peanuts, Name = "Peanuts" },
            new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Type = AllergenType.Wheat, Name = "Wheat" },
            new() { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Type = AllergenType.Soybeans, Name = "Soybeans" },
            new() { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Type = AllergenType.Sesame, Name = "Sesame" },

            // Dietary
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Type = AllergenType.Vegan, Name = "Vegan" },
            new() { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Type = AllergenType.Vegetarian, Name = "Vegetarian" },
            new() { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Type = AllergenType.Pescatarian, Name = "Pescatarian" },
            new() { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Type = AllergenType.GlutenFree, Name = "Gluten Free" },

            // Religious
            new() { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Type = AllergenType.Kosher, Name = "Kosher" },
            new() { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Type = AllergenType.Halal, Name = "Halal" },

            // Additional
            new() { Id = Guid.Parse("12121212-1212-1212-1212-121212121212"), Type = AllergenType.ContainsAlcohol, Name = "Contains Alcohol" },
            new() { Id = Guid.Parse("13131313-1313-1313-1313-131313131313"), Type = AllergenType.Nightshades, Name = "Nightshades" },
            new() { Id = Guid.Parse("14141414-1414-1414-1414-141414141414"), Type = AllergenType.Sulfites, Name = "Sulfites" },
            new() { Id = Guid.Parse("15151515-1515-1515-1515-151515151515"), Type = AllergenType.AddedSugar, Name = "Added Sugar" },
        };

// LocationUser configuration        modelBuilder.Entity<LocationUser>(entity =>        {            entity.HasKey(e => e.Id);            entity.Property(e => e.UserId).IsRequired().HasMaxLength(256);            entity.Property(e => e.Role).IsRequired().HasConversion<string>();            entity.HasIndex(e => new { e.LocationId, e.UserId }).IsUnique();            entity.HasOne(e => e.Location).WithMany(l => l.LocationUsers).HasForeignKey(e => e.LocationId).OnDelete(DeleteBehavior.Cascade);        });
        modelBuilder.Entity<Allergen>().HasData(allergens);
    }
}