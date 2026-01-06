using Avalonia.Controls;
using Avalonia.Interactivity;
using Dfc.Desktop.ViewModels;

namespace Dfc.Desktop.Views;

public partial class OnboardingWindow : Window
{
    public OnboardingWindow()
    {
        InitializeComponent();
    }

    public OnboardingWindow(OnboardingViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Don't close here - let the App.axaml.cs handle closing
        // This prevents double-close which causes InvalidOperationException
    }

    private void OnNextOrFinishClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OnboardingViewModel viewModel)
        {
            if (viewModel.IsLastStep)
            {
                viewModel.FinishCommand.Execute(null);
            }
            else
            {
                viewModel.NextCommand.Execute(null);
            }
        }
    }

    private void OnQuitClick(object? sender, RoutedEventArgs e)
    {
        // Close the onboarding window and exit the application
        Close();

        // Exit the entire application
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
