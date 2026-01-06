using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Freecost.Core.Services;

namespace Freecost.Desktop.ViewModels;

public partial class DonationReminderViewModel : ObservableObject
{
    private const string PAYPAL_URL = "https://www.paypal.com/paypalme/FreeCostApp";

    [ObservableProperty]
    private DonationStats _stats = new();

    public event EventHandler? CloseRequested;

    public DonationReminderViewModel()
    {
        // Default stats for designer
    }

    public DonationReminderViewModel(DonationStats stats)
    {
        Stats = stats;
    }

    [RelayCommand]
    private void OpenPayPal()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PAYPAL_URL,
                UseShellExecute = true
            });
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Opened PayPal: {PAYPAL_URL}");

            // Close window after opening PayPal
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DonationReminder] Error opening PayPal: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
