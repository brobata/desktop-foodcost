using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Threading.Tasks;

namespace Dfc.Desktop.ViewModels;

public partial class AddEditLocationViewModel : ViewModelBase
{
    private readonly ILocationService _locationService;
    private readonly Action _closeWindow;
    private readonly Location? _existingLocation;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private string? _phone;

    [ObservableProperty]
    private bool _isActive = true;

    public bool IsEditMode => _existingLocation != null;
    public string WindowTitle => IsEditMode ? "Edit Location" : "Add New Location";
    public string SaveButtonText => IsEditMode ? "Save Changes" : "Create Location";

    /// <summary>
    /// Raised when a location is saved (created or updated)
    /// </summary>
    public event EventHandler<Location>? LocationSaved;

    public AddEditLocationViewModel(
        ILocationService locationService,
        Action closeWindow,
        Location? existingLocation = null)
    {
        _locationService = locationService;
        _closeWindow = closeWindow;
        _existingLocation = existingLocation;

        if (existingLocation != null)
        {
            Name = existingLocation.Name;
            Address = existingLocation.Address;
            Phone = existingLocation.Phone;
            IsActive = existingLocation.IsActive;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return; // Name is required
        }

        try
        {
            Location savedLocation;

            if (_existingLocation != null)
            {
                // Update existing
                _existingLocation.Name = Name.Trim();
                _existingLocation.Address = Address?.Trim();
                _existingLocation.Phone = Phone?.Trim();
                _existingLocation.IsActive = IsActive;
                _existingLocation.ModifiedAt = DateTime.UtcNow;

                savedLocation = await _locationService.UpdateLocationAsync(_existingLocation);
            }
            else
            {
                // Create new
                var newLocation = new Location
                {
                    Id = Guid.NewGuid(),
                    Name = Name.Trim(),
                    Address = Address?.Trim(),
                    Phone = Phone?.Trim(),
                    IsActive = IsActive,
                    UserId = null, // Local-only location
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                savedLocation = await _locationService.CreateLocationAsync(newLocation);
            }

            LocationSaved?.Invoke(this, savedLocation);
            _closeWindow();
        }
        catch (Exception)
        {
            // Handle error - could add error display here
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _closeWindow();
    }
}
