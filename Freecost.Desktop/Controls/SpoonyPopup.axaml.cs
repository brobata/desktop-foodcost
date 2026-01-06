using System;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Services;

namespace Freecost.Desktop.Controls;

public partial class SpoonyPopup : UserControl
{
    private readonly ISpoonyService? _spoonyService;

    public SpoonyPopup()
    {
        InitializeComponent();
        DataContext = new SpoonyViewModel();

        // Get SpoonyService and listen for enabled state changes
        _spoonyService = App.Services?.GetService(typeof(ISpoonyService)) as ISpoonyService;
        if (_spoonyService != null)
        {
            // Monitor IsSpoonyEnabled property changes
            if (_spoonyService is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnSpoonyServicePropertyChanged;
            }
        }
    }

    private void OnSpoonyServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ISpoonyService.IsSpoonyEnabled))
        {
            if (_spoonyService?.IsSpoonyEnabled == false)
            {
                // Hide immediately when disabled
                Hide();
            }
        }
    }

    public void ShowMessage(SpoonyMessage message)
    {
        if (DataContext is SpoonyViewModel vm)
        {
            vm.Title = message.Title;
            vm.Message = message.Message;
            vm.IsHappyMood = message.Mood == SpoonyMood.Happy || message.Mood == SpoonyMood.Excited;
            vm.IsVisible = true;
        }
    }

    public void Hide()
    {
        if (DataContext is SpoonyViewModel vm)
        {
            vm.IsVisible = false;
        }
    }
}

public partial class SpoonyViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = false;

    [ObservableProperty]
    private string _title = "Woof! I'm Spoony!";

    [ObservableProperty]
    private string _message = "I'm here to help you with Freecost!";

    [ObservableProperty]
    private bool _isHappyMood = true;

    [ObservableProperty]
    private bool _hasAction = false;

    [ObservableProperty]
    private string _actionButtonText = string.Empty;

    public event EventHandler? CloseRequested;
    public event EventHandler? ActionRequested;

    [RelayCommand]
    private void Close()
    {
        IsVisible = false;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Action()
    {
        ActionRequested?.Invoke(this, EventArgs.Empty);
    }
}
