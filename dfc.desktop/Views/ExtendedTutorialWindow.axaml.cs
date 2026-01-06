using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using Dfc.Desktop.ViewModels;
using System;
using System.ComponentModel;

namespace Dfc.Desktop.Views;

public partial class ExtendedTutorialWindow : Window
{
    private bool _isAnimating = false;

    public ExtendedTutorialWindow()
    {
        InitializeComponent();

        // Add keyboard shortcuts for navigation
        KeyDown += OnKeyDown;

        // Hook into DataContext changes to add animation support
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ExtendedTutorialViewModel viewModel)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Trigger confetti celebration
        if (e.PropertyName == nameof(ExtendedTutorialViewModel.ShowConfetti))
        {
            if (sender is ExtendedTutorialViewModel vm && vm.ShowConfetti)
            {
                var confetti = this.FindControl<Controls.ConfettiControl>("ConfettiOverlay");

                if (vm.IsGrandConfetti)
                {
                    // GRAND CONFETTI - All 6 modules complete!
                    // WAY MORE PARTICLES, shorter but INTENSE!
                    confetti?.Celebrate(particleCount: 500, durationMs: 3000);
                }
                else
                {
                    // Regular confetti for single module completion
                    confetti?.Celebrate(particleCount: 150, durationMs: 3000);
                }
            }
        }

        // Animate when CurrentStep changes
        if (e.PropertyName == nameof(ExtendedTutorialViewModel.CurrentStep) && !_isAnimating)
        {
            _isAnimating = true;

            try
            {
                var contentPanel = this.FindControl<StackPanel>("ContentPanel");
                if (contentPanel != null)
                {
                    // Fade out
                    var fadeOut = new Animation
                    {
                        Duration = TimeSpan.FromMilliseconds(200),
                        Easing = new CubicEaseInOut(),
                        Children =
                        {
                            new KeyFrame
                            {
                                Cue = new Cue(0.0),
                                Setters = { new Setter(OpacityProperty, 1.0) }
                            },
                            new KeyFrame
                            {
                                Cue = new Cue(1.0),
                                Setters = { new Setter(OpacityProperty, 0.0) }
                            }
                        }
                    };

                    await fadeOut.RunAsync(contentPanel);

                    // Fade in
                    var fadeIn = new Animation
                    {
                        Duration = TimeSpan.FromMilliseconds(200),
                        Easing = new CubicEaseInOut(),
                        Children =
                        {
                            new KeyFrame
                            {
                                Cue = new Cue(0.0),
                                Setters = { new Setter(OpacityProperty, 0.0) }
                            },
                            new KeyFrame
                            {
                                Cue = new Cue(1.0),
                                Setters = { new Setter(OpacityProperty, 1.0) }
                            }
                        }
                    };

                    await fadeIn.RunAsync(contentPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
            }
            finally
            {
                _isAnimating = false;
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ExtendedTutorialViewModel viewModel)
            return;

        // Allow Enter or Right Arrow to go next
        if (e.Key == Key.Enter || e.Key == Key.Right)
        {
            if (viewModel.NextCommand.CanExecute(null))
            {
                viewModel.NextCommand.Execute(null);
                e.Handled = true;
            }
        }
        // Allow Left Arrow to go back
        else if (e.Key == Key.Left)
        {
            if (viewModel.BackCommand.CanExecute(null))
            {
                viewModel.BackCommand.Execute(null);
                e.Handled = true;
            }
        }
        // Allow Escape to skip/close
        else if (e.Key == Key.Escape)
        {
            if (viewModel.SkipCommand.CanExecute(null))
            {
                viewModel.SkipCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
