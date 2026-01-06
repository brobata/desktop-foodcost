using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dfc.Core.Services;
using System.Collections.Generic;

namespace Dfc.Desktop.Controls;

public partial class ValidationPanel : UserControl
{
    public static readonly StyledProperty<ValidationResult?> ValidationResultProperty =
        AvaloniaProperty.Register<ValidationPanel, ValidationResult?>(nameof(ValidationResult));

    public ValidationResult? ValidationResult
    {
        get => GetValue(ValidationResultProperty);
        set => SetValue(ValidationResultProperty, value);
    }

    public ValidationPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValidationResultProperty)
        {
            UpdateValidationDisplay();
        }
    }

    private void UpdateValidationDisplay()
    {
        var errorsPanel = this.FindControl<Border>("ErrorsPanel");
        var warningsPanel = this.FindControl<Border>("WarningsPanel");
        var infosPanel = this.FindControl<Border>("InfosPanel");
        var errorsList = this.FindControl<ItemsControl>("ErrorsList");
        var warningsList = this.FindControl<ItemsControl>("WarningsList");
        var infosList = this.FindControl<ItemsControl>("InfosList");

        if (errorsPanel == null || warningsPanel == null || infosPanel == null ||
            errorsList == null || warningsList == null || infosList == null)
            return;

        if (ValidationResult == null)
        {
            errorsPanel.IsVisible = false;
            warningsPanel.IsVisible = false;
            infosPanel.IsVisible = false;
            return;
        }

        // Update errors
        if (ValidationResult.Errors.Count > 0)
        {
            errorsPanel.IsVisible = true;
            errorsList.ItemsSource = ValidationResult.Errors;
        }
        else
        {
            errorsPanel.IsVisible = false;
        }

        // Update warnings
        if (ValidationResult.Warnings.Count > 0)
        {
            warningsPanel.IsVisible = true;
            warningsList.ItemsSource = ValidationResult.Warnings;
        }
        else
        {
            warningsPanel.IsVisible = false;
        }

        // Update infos
        if (ValidationResult.Infos.Count > 0)
        {
            infosPanel.IsVisible = true;
            infosList.ItemsSource = ValidationResult.Infos;
        }
        else
        {
            infosPanel.IsVisible = false;
        }
    }
}
