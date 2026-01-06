using Avalonia.Controls;
using Dfc.Desktop.ViewModels;
using System;

namespace Dfc.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("LoginWindow: Constructor starting...");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("LoginWindow: InitializeComponent completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoginWindow: Constructor FAILED: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"LoginWindow: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public LoginWindow(LoginViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
