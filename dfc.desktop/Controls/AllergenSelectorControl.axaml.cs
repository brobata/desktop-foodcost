using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dfc.Core.Enums;
using Dfc.Core.Models;
using Dfc.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dfc.Desktop.Controls;

public partial class AllergenItemViewModel : ObservableObject
{
    public AllergenType AllergenType { get; }
    public string DisplayName { get; }

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isAutoDetected;
    [ObservableProperty] private string? _sourceIngredients;
    [ObservableProperty] private IBrush _checkboxBrush = new SolidColorBrush(Color.FromRgb(33, 33, 33));
    [ObservableProperty] private string _tooltipText = string.Empty;

    public RelayCommand ToggleCommand { get; }

    public AllergenItemViewModel(AllergenType allergenType, Action onToggle)
    {
        AllergenType = allergenType;
        DisplayName = GetDisplayName(allergenType);
        ToggleCommand = new RelayCommand(() =>
        {
            IsSelected = !IsSelected;
            UpdateColor();
            onToggle();
        });

        UpdateColor();
    }

    partial void OnIsSelectedChanged(bool value)
    {
        UpdateColor();
        UpdateTooltip();
    }

    partial void OnIsAutoDetectedChanged(bool value)
    {
        UpdateColor();
        UpdateTooltip();
    }

    partial void OnSourceIngredientsChanged(string? value)
    {
        UpdateTooltip();
    }

    private void UpdateColor()
    {
        if (IsAutoDetected && IsSelected)
        {
            // Green - auto-detected from ingredient and checked
            CheckboxBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4CAF50
        }
        else if (IsAutoDetected && !IsSelected)
        {
            // Red - auto-detected but unchecked (WARNING: possible error!)
            CheckboxBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // #F44336
        }
        else if (!IsAutoDetected && IsSelected)
        {
            // Green - manually added at recipe/entree level and checked
            CheckboxBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // #4CAF50
        }
        else
        {
            // Black - not checked and not auto-detected (default state)
            CheckboxBrush = new SolidColorBrush(Color.FromRgb(33, 33, 33)); // #212121
        }
    }

    private void UpdateTooltip()
    {
        if (IsAutoDetected && IsSelected)
        {
            // Auto-detected and enabled
            if (!string.IsNullOrEmpty(SourceIngredients))
            {
                TooltipText = $"Auto-detected from: {SourceIngredients}";
            }
            else
            {
                TooltipText = "Present in this item";
            }
        }
        else if (IsAutoDetected && !IsSelected)
        {
            // Auto-detected but disabled - WARNING!
            if (!string.IsNullOrEmpty(SourceIngredients))
            {
                TooltipText = $"⚠️ WARNING: Auto-detected from {SourceIngredients} but you unchecked it!";
            }
            else
            {
                TooltipText = "⚠️ WARNING: Auto-detected but unchecked!";
            }
        }
        else if (!IsAutoDetected && IsSelected)
        {
            // Manually added
            TooltipText = "Manually added";
        }
        else
        {
            // Not checked and not auto-detected
            TooltipText = "Not present";
        }
    }

    private string GetDisplayName(AllergenType type)
    {
        return type switch
        {
            AllergenType.TreeNuts => "Tree Nuts",
            AllergenType.GlutenFree => "Gluten Free",
            AllergenType.ContainsAlcohol => "Contains Alcohol",
            AllergenType.AddedSugar => "Added Sugar",
            _ => type.ToString()
        };
    }
}

public partial class AllergenSelectorControl : UserControl
{
    public static readonly StyledProperty<ObservableCollection<AllergenItemViewModel>> AllergensProperty =
        AvaloniaProperty.Register<AllergenSelectorControl, ObservableCollection<AllergenItemViewModel>>(
            nameof(Allergens),
            new ObservableCollection<AllergenItemViewModel>());

    public ObservableCollection<AllergenItemViewModel> Allergens
    {
        get => GetValue(AllergensProperty);
        set => SetValue(AllergensProperty, value);
    }

    public event EventHandler? AllergensChanged;

    public AllergenSelectorControl()
    {
        InitializeComponent();
        InitializeAllergens();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var grid = this.FindControl<ItemsControl>("AllergenGrid");
        if (grid != null)
        {
            grid.ItemsSource = Allergens;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AllergensProperty)
        {
            var grid = this.FindControl<ItemsControl>("AllergenGrid");
            if (grid != null)
            {
                grid.ItemsSource = Allergens;
            }
        }
    }

    private void InitializeAllergens()
    {
        var allergens = new ObservableCollection<AllergenItemViewModel>();

        // Add all allergen types
        foreach (AllergenType type in Enum.GetValues<AllergenType>())
        {
            allergens.Add(new AllergenItemViewModel(type, () => AllergensChanged?.Invoke(this, EventArgs.Empty)));
        }

        Allergens = allergens;
    }

    public void SetAutoDetectedAllergens(Dictionary<AllergenType, List<string>> detectedAllergens)
    {
        foreach (var allergen in Allergens)
        {
            if (detectedAllergens.ContainsKey(allergen.AllergenType))
            {
                allergen.IsAutoDetected = true;
                allergen.IsSelected = true;
                allergen.SourceIngredients = string.Join(", ", detectedAllergens[allergen.AllergenType]);
            }
            else
            {
                allergen.IsAutoDetected = false;
                allergen.SourceIngredients = null;
            }
        }
    }

    public List<AllergenItemViewModel> GetSelectedAllergens()
    {
        return Allergens.Where(a => a.IsSelected).ToList();
    }
}
