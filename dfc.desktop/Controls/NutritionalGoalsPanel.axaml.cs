using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dfc.Core.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dfc.Desktop.Controls;

public partial class NutritionalGoalsPanel : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<List<NutritionalGoalComparison>?> ComparisonsProperty =
        AvaloniaProperty.Register<NutritionalGoalsPanel, List<NutritionalGoalComparison>?>(nameof(Comparisons));

    public List<NutritionalGoalComparison>? Comparisons
    {
        get => GetValue(ComparisonsProperty);
        set
        {
            SetValue(ComparisonsProperty, value);
            OnPropertyChangedCustom(nameof(HasComparisons));
        }
    }

    public bool HasComparisons => Comparisons != null && Comparisons.Any();

    public NutritionalGoalsPanel()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ComparisonsProperty)
        {
            OnPropertyChangedCustom(nameof(HasComparisons));
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChangedCustom([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
