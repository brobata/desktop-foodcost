using FluentAssertions;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Services;
using Dfc.Data.Services;
using Dfc.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.Tests.Services;

/// <summary>
/// Comprehensive tests for LocalModificationService delta sync functionality.
/// Tests smart conflict resolution, modification tracking, and sync operations.
/// </summary>
public class LocalModificationServiceTests : DatabaseTestBase
{
    private readonly LocalModificationService _service;
    private readonly Mock<ILogger<LocalModificationService>> _loggerMock;
    private readonly Guid _testLocationId = Guid.NewGuid();
    private readonly Guid _testEntityId = Guid.NewGuid();

    public LocalModificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<LocalModificationService>>();
        _service = new LocalModificationService(Context, _loggerMock.Object);

        // Create a test location to satisfy foreign key constraint
        var testLocation = new Location
        {
            Id = _testLocationId,
            Name = "Test Location",
            Address = "Test Address",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        Context.Locations.Add(testLocation);
        Context.SaveChanges();
    }

    #region TrackCreationAsync Tests

    [Fact]
    public async Task TrackCreationAsync_ShouldAddModification_WhenNoExistingModifications()
    {
        // Arrange
        var entityType = "Ingredient";

        // Act
        await _service.TrackCreationAsync(entityType, _testEntityId, _testLocationId);

        // Assert
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);

        var modification = modifications.First();
        modification.EntityType.Should().Be(entityType);
        modification.EntityId.Should().Be(_testEntityId);
        modification.LocationId.Should().Be(_testLocationId);
        modification.ModificationType.Should().Be(ModificationType.Create);
        modification.IsSynced.Should().BeFalse();
        modification.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TrackCreationAsync_ShouldRemoveExistingUnsynced_WhenModificationExists()
    {
        // Arrange
        var entityType = "Ingredient";

        // Add an existing unsynced update
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);
        var existingMods = await Context.LocalModifications.ToListAsync();
        existingMods.Should().HaveCount(1);

        // Act
        await _service.TrackCreationAsync(entityType, _testEntityId, _testLocationId);

        // Assert
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);
        modifications.First().ModificationType.Should().Be(ModificationType.Create);
    }

    [Fact]
    public async Task TrackCreationAsync_ShouldNotRemoveSyncedModifications()
    {
        // Arrange
        var entityType = "Ingredient";

        // Add a synced modification
        var syncedMod = new LocalModification
        {
            EntityType = entityType,
            EntityId = _testEntityId,
            LocationId = _testLocationId,
            ModificationType = ModificationType.Update,
            IsSynced = true,
            SyncedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        Context.LocalModifications.Add(syncedMod);
        await Context.SaveChangesAsync();

        // Act
        await _service.TrackCreationAsync(entityType, _testEntityId, _testLocationId);

        // Assert
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(2); // Synced + new Create
        modifications.Should().Contain(m => m.IsSynced);
        modifications.Should().Contain(m => !m.IsSynced && m.ModificationType == ModificationType.Create);
    }

    #endregion

    #region TrackUpdateAsync Tests

    [Fact]
    public async Task TrackUpdateAsync_ShouldAddModification_WhenNoExistingModifications()
    {
        // Arrange
        var entityType = "Recipe";

        // Act
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);

        // Assert
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);

        var modification = modifications.First();
        modification.ModificationType.Should().Be(ModificationType.Update);
        modification.IsSynced.Should().BeFalse();
    }

    [Fact]
    public async Task TrackUpdateAsync_ShouldKeepCreate_WhenUnsyncedCreateExists()
    {
        // Arrange - SMART CONFLICT RESOLUTION TEST
        var entityType = "Recipe";

        // First, track creation
        await _service.TrackCreationAsync(entityType, _testEntityId, _testLocationId);
        var createTime = (await Context.LocalModifications.FirstAsync()).ModifiedAt;

        // Wait a moment to ensure time difference
        await Task.Delay(50);

        // Act - Track update on same entity
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);

        // Assert - Should still be Create, not Update
        var modifications = await Context.LocalModifications.Where(m => !m.IsSynced).ToListAsync();
        modifications.Should().HaveCount(1);

        var modification = modifications.First();
        modification.ModificationType.Should().Be(ModificationType.Create,
            "Create + Update should consolidate to Create");
        modification.ModifiedAt.Should().BeAfter(createTime,
            "ModifiedAt timestamp should be updated");
    }

    [Fact]
    public async Task TrackUpdateAsync_ShouldReplaceExistingUpdate_WhenUnsyncedUpdateExists()
    {
        // Arrange
        var entityType = "Entree";

        // First update
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);
        var firstUpdateId = (await Context.LocalModifications.FirstAsync()).Id;

        await Task.Delay(50);

        // Act - Second update
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);

        // Assert - Should consolidate multiple updates into one
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);
        modifications.First().Id.Should().NotBe(firstUpdateId,
            "Old update should be replaced with new one");
        modifications.First().ModificationType.Should().Be(ModificationType.Update);
    }

    #endregion

    #region TrackDeletionAsync Tests

    [Fact]
    public async Task TrackDeletionAsync_ShouldAddDeletion_WhenNoExistingModifications()
    {
        // Arrange
        var entityType = "Ingredient";

        // Act
        await _service.TrackDeletionAsync(entityType, _testEntityId, _testLocationId);

        // Assert
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);
        modifications.First().ModificationType.Should().Be(ModificationType.Delete);
    }

    [Fact]
    public async Task TrackDeletionAsync_ShouldRemoveCreate_WhenUnsyncedCreateExists()
    {
        // Arrange - SMART CONFLICT RESOLUTION TEST
        var entityType = "Recipe";

        // Track creation (entity never existed remotely)
        await _service.TrackCreationAsync(entityType, _testEntityId, _testLocationId);

        // Act - Delete the entity
        await _service.TrackDeletionAsync(entityType, _testEntityId, _testLocationId);

        // Assert - Should track deletion since we don't know if it was synced
        // Actually, based on the implementation, it removes create and adds delete
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);
        modifications.First().ModificationType.Should().Be(ModificationType.Delete);
    }

    [Fact]
    public async Task TrackDeletionAsync_ShouldReplaceUpdate_WhenUnsyncedUpdateExists()
    {
        // Arrange
        var entityType = "Entree";

        // Track update
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);

        // Act - Delete the entity
        await _service.TrackDeletionAsync(entityType, _testEntityId, _testLocationId);

        // Assert - Delete supersedes Update
        var modifications = await Context.LocalModifications.ToListAsync();
        modifications.Should().HaveCount(1);
        modifications.First().ModificationType.Should().Be(ModificationType.Delete);
    }

    [Fact]
    public async Task TrackDeletionAsync_ShouldRemoveAllUnsynced_BeforeAddingDeletion()
    {
        // Arrange
        var entityType = "Recipe";

        // Add multiple unsynced modifications
        await _service.TrackCreationAsync(entityType, _testEntityId, _testLocationId);
        await _service.TrackUpdateAsync(entityType, Guid.NewGuid(), _testLocationId);
        await _service.TrackUpdateAsync(entityType, _testEntityId, _testLocationId);

        // Act
        await _service.TrackDeletionAsync(entityType, _testEntityId, _testLocationId);

        // Assert - Only deletion for _testEntityId should remain
        var modifications = await Context.LocalModifications
            .Where(m => m.EntityId == _testEntityId)
            .ToListAsync();
        modifications.Should().HaveCount(1);
        modifications.First().ModificationType.Should().Be(ModificationType.Delete);
    }

    #endregion

    #region GetUnsyncedModificationsAsync Tests

    [Fact]
    public async Task GetUnsyncedModificationsAsync_ShouldReturnEmpty_WhenNoModifications()
    {
        // Act
        var result = await _service.GetUnsyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnsyncedModificationsAsync_ShouldReturnOnlyUnsynced()
    {
        // Arrange
        await _service.TrackCreationAsync("Ingredient", Guid.NewGuid(), _testLocationId);
        await _service.TrackUpdateAsync("Recipe", Guid.NewGuid(), _testLocationId);

        // Mark first as synced
        var firstMod = await Context.LocalModifications.FirstAsync();
        await _service.MarkAsSyncedAsync(firstMod.Id);

        // Act
        var result = await _service.GetUnsyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsSynced.Should().BeFalse();
    }

    [Fact]
    public async Task GetUnsyncedModificationsAsync_ShouldReturnOrderedByModifiedAt()
    {
        // Arrange
        var oldTime = DateTime.UtcNow.AddMinutes(-10);
        var midTime = DateTime.UtcNow.AddMinutes(-5);
        var newTime = DateTime.UtcNow;

        var mod1 = new LocalModification
        {
            EntityType = "Ingredient",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Create,
            ModifiedAt = newTime,
            IsSynced = false
        };
        var mod2 = new LocalModification
        {
            EntityType = "Recipe",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Update,
            ModifiedAt = oldTime,
            IsSynced = false
        };
        var mod3 = new LocalModification
        {
            EntityType = "Entree",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Delete,
            ModifiedAt = midTime,
            IsSynced = false
        };

        Context.LocalModifications.AddRange(mod1, mod2, mod3);
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.GetUnsyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().HaveCount(3);
        result[0].ModifiedAt.Should().Be(oldTime);
        result[1].ModifiedAt.Should().Be(midTime);
        result[2].ModifiedAt.Should().Be(newTime);
    }

    [Fact]
    public async Task GetUnsyncedModificationsAsync_ShouldFilterByLocation()
    {
        // Arrange
        var otherLocationId = Guid.NewGuid();

        // Create other location for foreign key constraint
        var otherLocation = new Location
        {
            Id = otherLocationId,
            Name = "Other Location",
            Address = "Other Address",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        Context.Locations.Add(otherLocation);
        await Context.SaveChangesAsync();

        await _service.TrackCreationAsync("Ingredient", Guid.NewGuid(), _testLocationId);
        await _service.TrackUpdateAsync("Recipe", Guid.NewGuid(), otherLocationId);

        // Act
        var result = await _service.GetUnsyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().HaveCount(1);
        result.First().LocationId.Should().Be(_testLocationId);
    }

    #endregion

    #region GetSyncedModificationsAsync Tests

    [Fact]
    public async Task GetSyncedModificationsAsync_ShouldReturnEmpty_WhenNoSyncedModifications()
    {
        // Arrange
        await _service.TrackCreationAsync("Ingredient", Guid.NewGuid(), _testLocationId);

        // Act
        var result = await _service.GetSyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSyncedModificationsAsync_ShouldReturnOnlySynced()
    {
        // Arrange
        await _service.TrackCreationAsync("Ingredient", Guid.NewGuid(), _testLocationId);
        await _service.TrackUpdateAsync("Recipe", Guid.NewGuid(), _testLocationId);

        var firstMod = await Context.LocalModifications.FirstAsync();
        await _service.MarkAsSyncedAsync(firstMod.Id);

        // Act
        var result = await _service.GetSyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().HaveCount(1);
        result.First().IsSynced.Should().BeTrue();
    }

    [Fact]
    public async Task GetSyncedModificationsAsync_ShouldRespectLimit()
    {
        // Arrange
        var modificationIds = new List<Guid>();
        for (int i = 0; i < 150; i++)
        {
            await _service.TrackCreationAsync("Ingredient", Guid.NewGuid(), _testLocationId);
        }

        // Get all unsynced modifications and mark them as synced
        var allMods = await Context.LocalModifications.Where(m => !m.IsSynced).ToListAsync();
        foreach (var mod in allMods)
        {
            await _service.MarkAsSyncedAsync(mod.Id);
        }

        // Act
        var result = await _service.GetSyncedModificationsAsync(_testLocationId, 50);

        // Assert
        result.Should().HaveCount(50);
    }

    [Fact]
    public async Task GetSyncedModificationsAsync_ShouldReturnOrderedByNewest()
    {
        // Arrange
        var times = new[]
        {
            DateTime.UtcNow.AddHours(-3),
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddHours(-1)
        };

        foreach (var time in times)
        {
            var mod = new LocalModification
            {
                EntityType = "Ingredient",
                EntityId = Guid.NewGuid(),
                LocationId = _testLocationId,
                ModificationType = ModificationType.Create,
                ModifiedAt = time,
                IsSynced = true,
                SyncedAt = time.AddMinutes(5)
            };
            Context.LocalModifications.Add(mod);
        }
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.GetSyncedModificationsAsync(_testLocationId);

        // Assert
        result.Should().HaveCount(3);
        result[0].SyncedAt.Should().BeAfter(result[1].SyncedAt!.Value);
        result[1].SyncedAt.Should().BeAfter(result[2].SyncedAt!.Value);
    }

    #endregion

    #region MarkAsSyncedAsync Tests

    [Fact]
    public async Task MarkAsSyncedAsync_ShouldSetSyncedProperties()
    {
        // Arrange
        await _service.TrackCreationAsync("Ingredient", _testEntityId, _testLocationId);
        var mod = await Context.LocalModifications.FirstAsync();

        // Act
        await _service.MarkAsSyncedAsync(mod.Id);

        // Assert
        var updated = await Context.LocalModifications.FindAsync(mod.Id);
        updated.Should().NotBeNull();
        updated!.IsSynced.Should().BeTrue();
        updated.SyncedAt.Should().NotBeNull();
        updated.SyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updated.LastSyncError.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsSyncedAsync_ShouldClearPreviousErrors()
    {
        // Arrange
        await _service.TrackCreationAsync("Recipe", _testEntityId, _testLocationId);
        var mod = await Context.LocalModifications.FirstAsync();

        // Record a failure first
        await _service.RecordSyncFailureAsync(mod.Id, "Test error message");

        // Act
        await _service.MarkAsSyncedAsync(mod.Id);

        // Assert
        var updated = await Context.LocalModifications.FindAsync(mod.Id);
        updated!.LastSyncError.Should().BeNull();
        updated.IsSynced.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsSyncedAsync_ShouldDoNothing_WhenModificationNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        await _service.MarkAsSyncedAsync(nonExistentId);

        // Assert - Should not throw
        var mods = await Context.LocalModifications.ToListAsync();
        mods.Should().BeEmpty();
    }

    #endregion

    #region RecordSyncFailureAsync Tests

    [Fact]
    public async Task RecordSyncFailureAsync_ShouldIncrementAttempts()
    {
        // Arrange
        await _service.TrackCreationAsync("Ingredient", _testEntityId, _testLocationId);
        var mod = await Context.LocalModifications.FirstAsync();

        // Act
        await _service.RecordSyncFailureAsync(mod.Id, "First failure");
        await _service.RecordSyncFailureAsync(mod.Id, "Second failure");
        await _service.RecordSyncFailureAsync(mod.Id, "Third failure");

        // Assert
        var updated = await Context.LocalModifications.FindAsync(mod.Id);
        updated!.SyncAttempts.Should().Be(3);
    }

    [Fact]
    public async Task RecordSyncFailureAsync_ShouldStoreErrorMessage()
    {
        // Arrange
        await _service.TrackCreationAsync("Recipe", _testEntityId, _testLocationId);
        var mod = await Context.LocalModifications.FirstAsync();
        var errorMessage = "Network connection failed";

        // Act
        await _service.RecordSyncFailureAsync(mod.Id, errorMessage);

        // Assert
        var updated = await Context.LocalModifications.FindAsync(mod.Id);
        updated!.LastSyncError.Should().Be(errorMessage);
    }

    [Fact]
    public async Task RecordSyncFailureAsync_ShouldTruncateLongErrors()
    {
        // Arrange
        await _service.TrackCreationAsync("Entree", _testEntityId, _testLocationId);
        var mod = await Context.LocalModifications.FirstAsync();
        var longError = new string('X', 1500); // 1500 characters

        // Act
        await _service.RecordSyncFailureAsync(mod.Id, longError);

        // Assert
        var updated = await Context.LocalModifications.FindAsync(mod.Id);
        updated!.LastSyncError.Should().HaveLength(1000);
    }

    [Fact]
    public async Task RecordSyncFailureAsync_ShouldDoNothing_WhenModificationNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        await _service.RecordSyncFailureAsync(nonExistentId, "Error");

        // Assert - Should not throw
        var mods = await Context.LocalModifications.ToListAsync();
        mods.Should().BeEmpty();
    }

    #endregion

    #region ClearOldSyncedModificationsAsync Tests

    [Fact]
    public async Task ClearOldSyncedModificationsAsync_ShouldRemoveOldSynced()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        // Add old synced modification
        var oldMod = new LocalModification
        {
            EntityType = "Ingredient",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Create,
            ModifiedAt = cutoffDate.AddDays(-1),
            IsSynced = true,
            SyncedAt = cutoffDate.AddDays(-1)
        };
        Context.LocalModifications.Add(oldMod);
        await Context.SaveChangesAsync();

        // Act
        await _service.ClearOldSyncedModificationsAsync(cutoffDate);

        // Assert
        var remaining = await Context.LocalModifications.ToListAsync();
        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearOldSyncedModificationsAsync_ShouldKeepRecentSynced()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        // Add recent synced modification
        var recentMod = new LocalModification
        {
            EntityType = "Recipe",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Update,
            ModifiedAt = cutoffDate.AddDays(1),
            IsSynced = true,
            SyncedAt = cutoffDate.AddDays(1)
        };
        Context.LocalModifications.Add(recentMod);
        await Context.SaveChangesAsync();

        // Act
        await _service.ClearOldSyncedModificationsAsync(cutoffDate);

        // Assert
        var remaining = await Context.LocalModifications.ToListAsync();
        remaining.Should().HaveCount(1);
    }

    [Fact]
    public async Task ClearOldSyncedModificationsAsync_ShouldNotRemoveUnsynced()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        // Add old unsynced modification
        var unsyncedMod = new LocalModification
        {
            EntityType = "Entree",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Create,
            ModifiedAt = cutoffDate.AddDays(-10),
            IsSynced = false
        };
        Context.LocalModifications.Add(unsyncedMod);
        await Context.SaveChangesAsync();

        // Act
        await _service.ClearOldSyncedModificationsAsync(cutoffDate);

        // Assert
        var remaining = await Context.LocalModifications.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining.First().IsSynced.Should().BeFalse();
    }

    [Fact]
    public async Task ClearOldSyncedModificationsAsync_ShouldHandleEmptyTable()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        // Act
        await _service.ClearOldSyncedModificationsAsync(cutoffDate);

        // Assert - Should not throw
        var remaining = await Context.LocalModifications.ToListAsync();
        remaining.Should().BeEmpty();
    }

    #endregion

    #region GetLastSyncTimestampAsync Tests

    [Fact]
    public async Task GetLastSyncTimestampAsync_ShouldReturnNull_WhenNoSyncedModifications()
    {
        // Act
        var result = await _service.GetLastSyncTimestampAsync(_testLocationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLastSyncTimestampAsync_ShouldReturnMostRecent()
    {
        // Arrange
        var times = new[]
        {
            DateTime.UtcNow.AddHours(-3),
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(-2)
        };

        foreach (var time in times)
        {
            var mod = new LocalModification
            {
                EntityType = "Ingredient",
                EntityId = Guid.NewGuid(),
                LocationId = _testLocationId,
                ModificationType = ModificationType.Create,
                ModifiedAt = time,
                IsSynced = true,
                SyncedAt = time.AddMinutes(5)
            };
            Context.LocalModifications.Add(mod);
        }
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.GetLastSyncTimestampAsync(_testLocationId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeCloseTo(DateTime.UtcNow.AddHours(-1).AddMinutes(5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetLastSyncTimestampAsync_ShouldFilterByLocation()
    {
        // Arrange
        var otherLocationId = Guid.NewGuid();

        // Create other location for foreign key constraint
        var otherLocation = new Location
        {
            Id = otherLocationId,
            Name = "Other Location",
            Address = "Other Address",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        Context.Locations.Add(otherLocation);
        await Context.SaveChangesAsync();

        var testTime = DateTime.UtcNow.AddHours(-1);
        var otherTime = DateTime.UtcNow.AddMinutes(-30);

        var testMod = new LocalModification
        {
            EntityType = "Ingredient",
            EntityId = Guid.NewGuid(),
            LocationId = _testLocationId,
            ModificationType = ModificationType.Create,
            ModifiedAt = testTime,
            IsSynced = true,
            SyncedAt = testTime
        };
        var otherMod = new LocalModification
        {
            EntityType = "Recipe",
            EntityId = Guid.NewGuid(),
            LocationId = otherLocationId,
            ModificationType = ModificationType.Update,
            ModifiedAt = otherTime,
            IsSynced = true,
            SyncedAt = otherTime
        };
        Context.LocalModifications.AddRange(testMod, otherMod);
        await Context.SaveChangesAsync();

        // Act
        var result = await _service.GetLastSyncTimestampAsync(_testLocationId);

        // Assert
        result.Should().BeCloseTo(testTime, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task ComplexScenario_CreateUpdateDelete_ShouldConsolidate()
    {
        // Arrange
        var entityType = "Recipe";
        var entityId = Guid.NewGuid();

        // Act - Simulate full lifecycle
        await _service.TrackCreationAsync(entityType, entityId, _testLocationId);
        await _service.TrackUpdateAsync(entityType, entityId, _testLocationId);
        await _service.TrackUpdateAsync(entityType, entityId, _testLocationId);
        await _service.TrackDeletionAsync(entityType, entityId, _testLocationId);

        // Assert
        var modifications = await Context.LocalModifications
            .Where(m => m.EntityId == entityId && !m.IsSynced)
            .ToListAsync();

        modifications.Should().HaveCount(1, "All operations should consolidate");
        modifications.First().ModificationType.Should().Be(ModificationType.Delete,
            "Final operation is deletion");
    }

    [Fact]
    public async Task ComplexScenario_MultipleEntities_ShouldTrackSeparately()
    {
        // Arrange
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();
        var entity3 = Guid.NewGuid();

        // Act
        await _service.TrackCreationAsync("Ingredient", entity1, _testLocationId);
        await _service.TrackUpdateAsync("Recipe", entity2, _testLocationId);
        await _service.TrackDeletionAsync("Entree", entity3, _testLocationId);

        // Assert
        var modifications = await _service.GetUnsyncedModificationsAsync(_testLocationId);
        modifications.Should().HaveCount(3);
        modifications.Should().Contain(m => m.EntityId == entity1 && m.ModificationType == ModificationType.Create);
        modifications.Should().Contain(m => m.EntityId == entity2 && m.ModificationType == ModificationType.Update);
        modifications.Should().Contain(m => m.EntityId == entity3 && m.ModificationType == ModificationType.Delete);
    }

    [Fact]
    public async Task ComplexScenario_SyncWorkflow_ShouldTrackCorrectly()
    {
        // Arrange - Simulate real sync workflow
        var entity1 = Guid.NewGuid();
        var entity2 = Guid.NewGuid();

        // User creates two items
        await _service.TrackCreationAsync("Ingredient", entity1, _testLocationId);
        await _service.TrackCreationAsync("Ingredient", entity2, _testLocationId);

        // Get unsynced for sync
        var unsynced1 = await _service.GetUnsyncedModificationsAsync(_testLocationId);
        unsynced1.Should().HaveCount(2);

        // Sync first succeeds
        await _service.MarkAsSyncedAsync(unsynced1[0].Id);

        // Sync second fails
        await _service.RecordSyncFailureAsync(unsynced1[1].Id, "Network error");

        // Check state
        var unsynced2 = await _service.GetUnsyncedModificationsAsync(_testLocationId);
        unsynced2.Should().HaveCount(1);
        unsynced2.First().SyncAttempts.Should().Be(1);

        // User makes changes while offline
        await _service.TrackUpdateAsync("Ingredient", entity1, _testLocationId); // Update synced item
        await _service.TrackDeletionAsync("Ingredient", entity2, _testLocationId); // Delete failed item

        // Final state
        var unsynced3 = await _service.GetUnsyncedModificationsAsync(_testLocationId);
        unsynced3.Should().HaveCount(2);
        unsynced3.Should().Contain(m => m.EntityId == entity1 && m.ModificationType == ModificationType.Update);
        unsynced3.Should().Contain(m => m.EntityId == entity2 && m.ModificationType == ModificationType.Delete);
    }

    #endregion
}
