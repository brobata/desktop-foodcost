using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Freecost.Core.Services;
using Freecost.Desktop.Controls;
using Freecost.Desktop.Services;
using Freecost.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Freecost.Desktop.Views;

public partial class MainWindow : Window
{
    private bool _needsRefresh = false;
    private StatusNotificationBar? _statusBar;
    private SpoonyPopup? _spoonyPopup;
    private readonly ILogger? _logger;
    private readonly ISpoonyService? _spoonyService;

    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        KeyDown += OnMainWindowKeyDown;
        Activated += OnWindowActivated;
        Opened += OnWindowOpened;
        _logger = App.Services?.GetService<ILogger<MainWindow>>();
        _spoonyService = App.Services?.GetService<ISpoonyService>();
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        // Get the status bar control
        _statusBar = this.FindControl<StatusNotificationBar>("StatusBar");

        // Get Spoony popup control
        _spoonyPopup = this.FindControl<SpoonyPopup>("SpoonyPopup");

        // Subscribe to notification service
        var notificationService = App.Services?.GetService<IStatusNotificationService>();
        if (notificationService != null)
        {
            notificationService.NotificationPosted += OnNotificationPosted;
        }

        // Subscribe to Spoony service
        if (_spoonyService != null && _spoonyPopup != null)
        {
            _spoonyService.SpoonyMessageRequested += OnSpoonyMessageRequested;

            // Show welcome message after a short delay
            System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _spoonyService.TriggerHelp(SpoonyHelpContext.Welcome);
                });
            });
        }
    }

    private void OnSpoonyMessageRequested(object? sender, SpoonyMessageEventArgs e)
    {
        _spoonyPopup?.ShowMessage(e.Message);
    }

    private void OnNotificationPosted(object? sender, StatusNotificationEventArgs e)
    {
        _statusBar?.ShowNotification(e);
    }

    private async void OnWindowActivated(object? sender, EventArgs e)
    {
        try
        {
            // Refresh the current view only when needed (e.g., after login/location selection)
            if (_needsRefresh && DataContext is MainWindowViewModel viewModel)
            {
                _needsRefresh = false;
                await viewModel.RefreshCurrentViewAsync();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error refreshing view on window activation");
        }
    }

    public void RequestRefresh()
    {
        _needsRefresh = true;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SetMainWindow(this);
        }
    }

    private async void OnShowKeyboardShortcutsClicked(object? sender, RoutedEventArgs e)
    {
        var dialog = new KeyboardShortcutsHelpWindow();
        await dialog.ShowDialog(this);
    }

    private async void OnShowAboutClicked(object? sender, RoutedEventArgs e)
    {
        var dialog = new AboutWindow();
        await dialog.ShowDialog(this);
    }

    private async void OnGlobalSearchClicked(object? sender, RoutedEventArgs e)
    {
        await ShowGlobalSearchAsync();
    }

    private async void OnMainWindowKeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+K = Global Search
        if (e.Key == Key.K && e.KeyModifiers == KeyModifiers.Control)
        {
            await ShowGlobalSearchAsync();
            e.Handled = true;
            return;
        }

        // Ctrl+F = Focus search box in current view
        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            // Find the content control that displays CurrentView
            var contentControl = this.FindControl<ContentControl>("CurrentViewHost");
            if (contentControl?.Content is Control currentView)
            {
                if (Services.FocusManager.FocusSearchBox(currentView))
                {
                    e.Handled = true;
                }
            }
            return;
        }

        // F1 or Ctrl+? = Show keyboard shortcuts help
        if (e.Key == Key.F1 || (e.Key == Key.OemQuestion && e.KeyModifiers == KeyModifiers.Control))
        {
            var dialog = new KeyboardShortcutsHelpWindow();
            await dialog.ShowDialog(this);
            e.Handled = true;
            return;
        }

        // Ctrl+1 through Ctrl+8 = Navigate to views
        if (e.KeyModifiers == KeyModifiers.Control && DataContext is MainWindowViewModel vm)
        {
            switch (e.Key)
            {
                case Key.D1:
                    vm.NavigateToDashboardCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D2:
                    vm.NavigateToIngredientsCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D3:
                    vm.NavigateToRecipesCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D4:
                    vm.NavigateToEntreesCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D5:
                    vm.NavigateToMenuPlanCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.D6:
                    vm.NavigateToSettingsCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        // Alt+Left/Right = Navigate between views
        if (e.KeyModifiers == KeyModifiers.Alt && DataContext is MainWindowViewModel viewModel)
        {
            if (e.Key == Key.Left)
            {
                // TODO: Navigate to previous view in history
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                // TODO: Navigate to next view in history
                e.Handled = true;
            }
        }
    }

    private async System.Threading.Tasks.Task ShowGlobalSearchAsync()
    {
        try
        {
            var serviceProvider = App.Services;
            if (serviceProvider == null) return;

            var ingredientService = serviceProvider.GetRequiredService<IIngredientService>();
            var recipeService = serviceProvider.GetRequiredService<IRecipeService>();
            var entreeService = serviceProvider.GetRequiredService<IEntreeService>();

            var dialog = new GlobalSearchWindow();
            var viewModel = new GlobalSearchViewModel(ingredientService, recipeService, entreeService);
            dialog.DataContext = viewModel;

            await dialog.ShowDialog(this);

            // Handle navigation to selected result
            if (dialog.SelectedResult != null && DataContext is MainWindowViewModel mainViewModel)
            {
                switch (dialog.SelectedResult.EntityType)
                {
                    case SearchResultEntityType.Ingredient:
                        mainViewModel.NavigateToIngredientsCommand.Execute(null);
                        break;
                    case SearchResultEntityType.Recipe:
                        mainViewModel.NavigateToRecipesCommand.Execute(null);
                        break;
                    case SearchResultEntityType.Entree:
                        mainViewModel.NavigateToEntreesCommand.Execute(null);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error showing global search dialog");
        }
    }
}