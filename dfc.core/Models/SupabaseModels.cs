using System;
using Dfc.Core.Enums;
using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Dfc.Core.Models;

/// <summary>
/// Base model for all Supabase entities
/// Includes common fields that all tables share
/// </summary>
public abstract class SupabaseBaseModel : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("modified_at")]
    public DateTime ModifiedAt { get; set; }

    [Column("row_version")]
    public int RowVersion { get; set; }
}

/// <summary>
/// Base model for entities that belong to a location (multi-tenant)
/// </summary>
public abstract class SupabaseLocationBaseModel : SupabaseBaseModel
{
    [Column("location_id")]
    public Guid LocationId { get; set; }
}

#region Core Entity Models

/// <summary>
/// Supabase model for Location entity
/// Maps to 'locations' table in PostgreSQL
/// </summary>
[Table("locations")]
public class SupabaseLocation : SupabaseBaseModel
{
    [Column("user_id")]
    public string? UserId { get; set; } // Supabase Auth UID (nullable for offline locations)

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    public string? Address { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Supabase model for Ingredient entity
/// Maps to 'ingredients' table in PostgreSQL
/// </summary>
[Table("ingredients")]
public class SupabaseIngredient : SupabaseLocationBaseModel
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("unit")]
    public int Unit { get; set; }

    [Column("current_price")]
    public decimal CurrentPrice { get; set; }

    [Column("case_quantity")]
    public decimal? CaseQuantity { get; set; }

    [Column("vendor_name")]
    public string? VendorName { get; set; }

    [Column("vendor_sku")]
    public string? VendorSku { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("category_color")]
    public string? CategoryColor { get; set; }

    [Column("use_alternate_unit")]
    public bool? UseAlternateUnit { get; set; }

    [Column("alternate_unit")]
    public int? AlternateUnit { get; set; }

    [Column("alternate_conversion_quantity")]
    public decimal? AlternateConversionQuantity { get; set; }

    [Column("alternate_conversion_unit")]
    public int? AlternateConversionUnit { get; set; }

    [Column("calories_per_unit")]
    public decimal? CaloriesPerUnit { get; set; }

    [Column("protein_per_unit")]
    public decimal? ProteinPerUnit { get; set; }

    [Column("carbohydrates_per_unit")]
    public decimal? CarbohydratesPerUnit { get; set; }

    [Column("fat_per_unit")]
    public decimal? FatPerUnit { get; set; }

    [Column("fiber_per_unit")]
    public decimal? FiberPerUnit { get; set; }

    [Column("sugar_per_unit")]
    public decimal? SugarPerUnit { get; set; }

    [Column("sodium_per_unit")]
    public decimal? SodiumPerUnit { get; set; }

    [Column("row_version")]
    public new int? RowVersion { get; set; }
}

/// <summary>
/// Supabase model for Recipe entity
/// Maps to 'recipes' table in PostgreSQL
/// </summary>
[Table("recipes")]
public class SupabaseRecipe : SupabaseLocationBaseModel
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("instructions")]
    public string? Instructions { get; set; }

    [Column("yield")]
    public decimal Yield { get; set; }

    [Column("yield_unit")]
    public string YieldUnit { get; set; } = string.Empty;

    [Column("prep_time_minutes")]
    public int? PrepTimeMinutes { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("is_shared")]
    public bool IsShared { get; set; } = false;

    [Column("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Supabase model for Entree entity
/// Maps to 'entrees' table in PostgreSQL
/// </summary>
[Table("entrees")]
public class SupabaseEntree : SupabaseLocationBaseModel
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("menu_price")]
    public decimal? MenuPrice { get; set; }

    [Column("servings_per_batch")]
    public decimal ServingsPerBatch { get; set; } = 1;

    [Column("notes")]
    public string? Notes { get; set; }
}

#endregion

#region Junction Table Models

/// <summary>
/// Supabase model for RecipeIngredient junction table
/// Maps to 'recipe_ingredients' table in PostgreSQL
/// </summary>
[Table("recipe_ingredients")]
public class SupabaseRecipeIngredient : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    [Column("ingredient_id")]
    public Guid IngredientId { get; set; }

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }
}

/// <summary>
/// Supabase model for EntreeIngredient junction table
/// Maps to 'entree_ingredients' table in PostgreSQL
/// </summary>
[Table("entree_ingredients")]
public class SupabaseEntreeIngredient : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("entree_id")]
    public Guid EntreeId { get; set; }

    [Column("ingredient_id")]
    public Guid IngredientId { get; set; }

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }
}

/// <summary>
/// Supabase model for EntreeRecipe junction table
/// Maps to 'entree_recipes' table in PostgreSQL
/// </summary>
[Table("entree_recipes")]
public class SupabaseEntreeRecipe : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("entree_id")]
    public Guid EntreeId { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    [Column("quantity")]
    public decimal Quantity { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }
}

#endregion

#region Supporting Models

/// <summary>
/// Supabase model for PriceHistory entity
/// Maps to 'price_history' table in PostgreSQL
/// </summary>
[Table("price_history")]
public class SupabasePriceHistory : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("ingredient_id")]
    public Guid IngredientId { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("recorded_at")]
    public DateTime RecordedAt { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Supabase model for Photo entity
/// Maps to 'photos' table in PostgreSQL
/// Stores metadata for photos in Supabase Storage with local caching
/// </summary>
[Table("photos")]
public class SupabasePhoto : SupabaseBaseModel
{
    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty; // 'Ingredient', 'Recipe', or 'Entree'

    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("storage_path")]
    public string StoragePath { get; set; } = string.Empty; // Path in Supabase Storage bucket

    [Column("public_url")]
    public string? PublicUrl { get; set; } // Public URL from Supabase Storage

    [Column("file_name")]
    public string FileName { get; set; } = string.Empty;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("caption")]
    public string? Caption { get; set; }

    [Column("sort_order")]
    public int? SortOrder { get; set; }
}


/// <summary>
/// Supabase model for LocationUser junction table
/// Maps to location_users table in PostgreSQL
/// </summary>
[Table("location_users")]
public class SupabaseLocationUser : SupabaseBaseModel
{
    [Column("location_id")]
    public Guid LocationId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = "viewer";
}

/// <summary>/// Supabase model for User entity/// Maps to 'users' table in PostgreSQL/// Mirrors Supabase auth users/// </summary>[Table("users")]public class SupabaseUser : SupabaseBaseModel{    [Column("supabase_auth_uid")]    public string SupabaseAuthUid { get; set; } = string.Empty;    [Column("email")]    public string Email { get; set; } = string.Empty;    [Column("role")]    public string Role { get; set; } = "viewer";    [Column("status")]    public string Status { get; set; } = "active";}/// <summary>/// Supabase model for IngredientAlias entity/// Maps to 'ingredient_aliases' table in PostgreSQL/// </summary>[Table("ingredient_aliases")]public class SupabaseIngredientAlias : BaseModel{    [PrimaryKey("id")]    public Guid Id { get; set; }    [Column("ingredient_id")]    public Guid IngredientId { get; set; }    [Column("alias_name")]    public string AliasName { get; set; } = string.Empty;    [Column("created_at")]    public DateTime CreatedAt { get; set; }}/// <summary>/// Supabase model for Allergen entity/// Maps to 'allergens' table in PostgreSQL/// </summary>[Table("allergens")]public class SupabaseAllergen : SupabaseBaseModel{    [Column("name")]    public string Name { get; set; } = string.Empty;    [Column("icon")]    public string? Icon { get; set; }}/// <summary>/// Supabase model for IngredientAllergen junction table/// Maps to 'ingredient_allergens' table in PostgreSQL/// </summary>[Table("ingredient_allergens")]public class SupabaseIngredientAllergen : BaseModel{    [PrimaryKey("id")]    public Guid Id { get; set; }    [Column("ingredient_id")]    public Guid IngredientId { get; set; }    [Column("allergen_id")]    public Guid AllergenId { get; set; }    [Column("created_at")]    public DateTime CreatedAt { get; set; }}/// <summary>/// Supabase model for RecipeAllergen junction table/// Maps to 'recipe_allergens' table in PostgreSQL/// </summary>[Table("recipe_allergens")]public class SupabaseRecipeAllergen : BaseModel{    [PrimaryKey("id")]    public Guid Id { get; set; }    [Column("recipe_id")]    public Guid RecipeId { get; set; }    [Column("allergen_id")]    public Guid AllergenId { get; set; }    [Column("created_at")]    public DateTime CreatedAt { get; set; }}/// <summary>/// Supabase model for EntreeAllergen junction table/// Maps to 'entree_allergens' table in PostgreSQL/// </summary>[Table("entree_allergens")]public class SupabaseEntreeAllergen : BaseModel{    [PrimaryKey("id")]    public Guid Id { get; set; }    [Column("entree_id")]    public Guid EntreeId { get; set; }    [Column("allergen_id")]    public Guid AllergenId { get; set; }    [Column("created_at")]    public DateTime CreatedAt { get; set; }}
#endregion

/// <summary>
/// Extension methods to convert between EF Core models and Supabase models
/// </summary>
public static class SupabaseModelExtensions
{
    public static SupabaseIngredient ToSupabase(this Ingredient ingredient)
    {
        return new SupabaseIngredient
        {
            Id = ingredient.Id,
            LocationId = ingredient.LocationId,
            Name = ingredient.Name,
            Unit = (int)ingredient.Unit,
            CurrentPrice = ingredient.CurrentPrice,
            CaseQuantity = ingredient.CaseQuantity,
            VendorName = ingredient.VendorName,
            VendorSku = ingredient.VendorSku,
            Category = ingredient.Category,
            CategoryColor = ingredient.CategoryColor,
            UseAlternateUnit = ingredient.UseAlternateUnit,
            AlternateUnit = ingredient.AlternateUnit.HasValue ? (int)ingredient.AlternateUnit.Value : null,
            AlternateConversionQuantity = ingredient.AlternateConversionQuantity,
            AlternateConversionUnit = ingredient.AlternateConversionUnit.HasValue ? (int)ingredient.AlternateConversionUnit.Value : null,
            CaloriesPerUnit = ingredient.CaloriesPerUnit,
            ProteinPerUnit = ingredient.ProteinPerUnit,
            CarbohydratesPerUnit = ingredient.CarbohydratesPerUnit,
            FatPerUnit = ingredient.FatPerUnit,
            FiberPerUnit = ingredient.FiberPerUnit,
            SugarPerUnit = ingredient.SugarPerUnit,
            SodiumPerUnit = ingredient.SodiumPerUnit,
            CreatedAt = ingredient.CreatedAt,
            ModifiedAt = ingredient.ModifiedAt,
            RowVersion = null // RowVersion is byte[] in EF Core but int in Supabase - skip for now
        };
    }

    public static Ingredient ToEfCore(this SupabaseIngredient supabaseIngredient)
    {
        return new Ingredient
        {
            Id = supabaseIngredient.Id,
            LocationId = supabaseIngredient.LocationId,
            Name = supabaseIngredient.Name,
            Unit = (UnitType)supabaseIngredient.Unit,
            CurrentPrice = supabaseIngredient.CurrentPrice,
            CaseQuantity = supabaseIngredient.CaseQuantity ?? 1,
            VendorName = supabaseIngredient.VendorName,
            VendorSku = supabaseIngredient.VendorSku,
            Category = supabaseIngredient.Category,
            CategoryColor = supabaseIngredient.CategoryColor,
            UseAlternateUnit = supabaseIngredient.UseAlternateUnit ?? false,
            AlternateUnit = supabaseIngredient.AlternateUnit.HasValue ? (UnitType)supabaseIngredient.AlternateUnit.Value : null,
            AlternateConversionQuantity = supabaseIngredient.AlternateConversionQuantity,
            AlternateConversionUnit = supabaseIngredient.AlternateConversionUnit.HasValue ? (UnitType)supabaseIngredient.AlternateConversionUnit.Value : null,
            CaloriesPerUnit = supabaseIngredient.CaloriesPerUnit,
            ProteinPerUnit = supabaseIngredient.ProteinPerUnit,
            CarbohydratesPerUnit = supabaseIngredient.CarbohydratesPerUnit,
            FatPerUnit = supabaseIngredient.FatPerUnit,
            FiberPerUnit = supabaseIngredient.FiberPerUnit,
            SugarPerUnit = supabaseIngredient.SugarPerUnit,
            SodiumPerUnit = supabaseIngredient.SodiumPerUnit,
            CreatedAt = supabaseIngredient.CreatedAt,
            ModifiedAt = supabaseIngredient.ModifiedAt,
            RowVersion = null // RowVersion is int in Supabase but byte[] in EF Core - skip for now
        };
    }

    public static SupabaseRecipe ToSupabase(this Recipe recipe)
    {
        return new SupabaseRecipe
        {
            Id = recipe.Id,
            LocationId = recipe.LocationId,
            Name = recipe.Name,
            Description = recipe.Description,
            Instructions = recipe.Instructions,
            Yield = recipe.Yield,
            YieldUnit = recipe.YieldUnit,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            Category = recipe.Category,
            IsShared = recipe.IsShared,
            Notes = recipe.Notes,
            CreatedAt = recipe.CreatedAt,
            ModifiedAt = recipe.ModifiedAt,
            RowVersion = 1 // Skip conversion - managed by database
        };
    }

    public static Recipe ToEfCore(this SupabaseRecipe supabaseRecipe)
    {
        return new Recipe
        {
            Id = supabaseRecipe.Id,
            LocationId = supabaseRecipe.LocationId,
            Name = supabaseRecipe.Name,
            Description = supabaseRecipe.Description,
            Instructions = supabaseRecipe.Instructions,
            Yield = supabaseRecipe.Yield,
            YieldUnit = supabaseRecipe.YieldUnit,
            PrepTimeMinutes = supabaseRecipe.PrepTimeMinutes,
            Category = supabaseRecipe.Category,
            IsShared = supabaseRecipe.IsShared,
            Notes = supabaseRecipe.Notes,
            CreatedAt = supabaseRecipe.CreatedAt,
            ModifiedAt = supabaseRecipe.ModifiedAt,
            RowVersion = null // Skip conversion - int in Supabase, byte[] in EF
        };
    }

    public static SupabaseEntree ToSupabase(this Entree entree)
    {
        return new SupabaseEntree
        {
            Id = entree.Id,
            LocationId = entree.LocationId,
            Name = entree.Name,
            Description = entree.Description,
            Category = entree.Category,
            MenuPrice = entree.MenuPrice,
            CreatedAt = entree.CreatedAt,
            ModifiedAt = entree.ModifiedAt,
            RowVersion = 1 // Skip conversion - managed by database
        };
    }

    public static Entree ToEfCore(this SupabaseEntree supabaseEntree)
    {
        return new Entree
        {
            Id = supabaseEntree.Id,
            LocationId = supabaseEntree.LocationId,
            Name = supabaseEntree.Name,
            Description = supabaseEntree.Description,
            Category = supabaseEntree.Category,
            MenuPrice = supabaseEntree.MenuPrice,
            CreatedAt = supabaseEntree.CreatedAt,
            ModifiedAt = supabaseEntree.ModifiedAt,
            RowVersion = null // Skip conversion - int in Supabase, byte[] in EF
        };
    }

    // Add more conversion methods for other entities as needed...
}
