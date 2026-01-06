using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecost.Desktop.Services;

/// <summary>
/// Interactive tutorial and guided tour service
/// </summary>
public interface ITutorialService
{
    void StartTutorial(string tutorialId);
    void ShowNextStep();
    void ShowPreviousStep();
    void EndTutorial();
    bool IsTutorialActive { get; }
    TutorialStep? CurrentStep { get; }
}

public class TutorialService : ITutorialService
{
    private Tutorial? _activeTutorial;
    private int _currentStepIndex;
    private readonly Dictionary<string, Tutorial> _tutorials = new();
    private Popup? _highlightPopup;
    private Border? _highlightBorder;
    private Window? _parentWindow;

    public bool IsTutorialActive => _activeTutorial != null;
    public TutorialStep? CurrentStep => _activeTutorial?.Steps.ElementAtOrDefault(_currentStepIndex);

    public TutorialService()
    {
        RegisterDefaultTutorials();
    }

    private void RegisterDefaultTutorials()
    {
        // Welcome Tutorial
        _tutorials["welcome"] = new Tutorial
        {
            Id = "welcome",
            Name = "Welcome to Freecost",
            Description = "Learn the basics of Freecost",
            Steps = new List<TutorialStep>
            {
                new()
                {
                    Title = "Welcome to Freecost! 👋",
                    Description = "This quick tour will show you how to get started with managing your restaurant costs.",
                    TargetControlName = null,
                    Position = TutorialPosition.Center
                },
                new()
                {
                    Title = "Navigation",
                    Description = "Use these buttons to navigate between different sections: Dashboard, Ingredients, Recipes, and Menu Items.",
                    TargetControlName = "NavigationPanel",
                    Position = TutorialPosition.Bottom,
                    HighlightPadding = new Thickness(8)
                },
                new()
                {
                    Title = "Keyboard Shortcuts",
                    Description = "Pro tip: Press Ctrl+1 through Ctrl+6 to quickly jump to any view. Press F1 anytime for help!",
                    TargetControlName = null,
                    Position = TutorialPosition.Center
                },
                new()
                {
                    Title = "Search Everything",
                    Description = "Press Ctrl+K to open global search and find ingredients, recipes, or menu items instantly.",
                    TargetControlName = "GlobalSearchButton",
                    Position = TutorialPosition.Bottom
                },
                new()
                {
                    Title = "You're All Set! 🎉",
                    Description = "You're ready to start managing your restaurant costs. Let's create your first ingredient!",
                    TargetControlName = null,
                    Position = TutorialPosition.Center
                }
            }
        };

        // Ingredients Tutorial
        _tutorials["ingredients"] = new Tutorial
        {
            Id = "ingredients",
            Name = "Managing Ingredients",
            Description = "Learn how to add and manage ingredients",
            Steps = new List<TutorialStep>
            {
                new()
                {
                    Title = "Add Your First Ingredient",
                    Description = "Click the 'Add Ingredient' button to create a new ingredient. You can also press Ctrl+N as a shortcut.",
                    TargetControlName = "AddIngredientButton",
                    Position = TutorialPosition.Bottom,
                    HighlightPadding = new Thickness(8)
                },
                new()
                {
                    Title = "Search and Filter",
                    Description = "Use the search box to quickly find ingredients. Press Ctrl+F to focus it anytime.",
                    TargetControlName = "SearchBox",
                    Position = TutorialPosition.Bottom
                },
                new()
                {
                    Title = "Track Price Changes",
                    Description = "Freecost automatically tracks price history. Click on any ingredient to see its price trends over time.",
                    TargetControlName = "IngredientsDataGrid",
                    Position = TutorialPosition.Top
                },
                new()
                {
                    Title = "Batch Operations",
                    Description = "Select multiple ingredients to delete, duplicate, or update them all at once.",
                    TargetControlName = "BatchToolbar",
                    Position = TutorialPosition.Bottom
                }
            }
        };

        // Recipes Tutorial
        _tutorials["recipes"] = new Tutorial
        {
            Id = "recipes",
            Name = "Creating Recipes",
            Description = "Learn how to build and cost recipes",
            Steps = new List<TutorialStep>
            {
                new()
                {
                    Title = "Create Your First Recipe",
                    Description = "Recipes are made up of ingredients. Click 'Add Recipe' to get started.",
                    TargetControlName = "AddRecipeButton",
                    Position = TutorialPosition.Bottom
                },
                new()
                {
                    Title = "Automatic Cost Calculation",
                    Description = "As you add ingredients, Freecost automatically calculates the total recipe cost and cost per serving.",
                    TargetControlName = null,
                    Position = TutorialPosition.Center
                },
                new()
                {
                    Title = "Profitability Analysis",
                    Description = "Set your menu price to see profitability metrics: food cost percentage, gross profit, and margin.",
                    TargetControlName = null,
                    Position = TutorialPosition.Center
                },
                new()
                {
                    Title = "Print Recipe Cards",
                    Description = "Print professional recipe cards for your kitchen. Press Ctrl+P when viewing a recipe.",
                    TargetControlName = null,
                    Position = TutorialPosition.Center
                }
            }
        };
    }

    public void StartTutorial(string tutorialId)
    {
        if (!_tutorials.TryGetValue(tutorialId, out var tutorial))
        {
            return;
        }

        _activeTutorial = tutorial;
        _currentStepIndex = 0;
        ShowCurrentStep();
    }

    public void ShowNextStep()
    {
        if (_activeTutorial == null) return;

        _currentStepIndex++;

        if (_currentStepIndex >= _activeTutorial.Steps.Count)
        {
            EndTutorial();
            return;
        }

        ShowCurrentStep();
    }

    public void ShowPreviousStep()
    {
        if (_activeTutorial == null || _currentStepIndex <= 0) return;

        _currentStepIndex--;
        ShowCurrentStep();
    }

    public void EndTutorial()
    {
        HideHighlight();
        _activeTutorial = null;
        _currentStepIndex = 0;
    }

    private void ShowCurrentStep()
    {
        var step = CurrentStep;
        if (step == null) return;

        // Find parent window
        if (_parentWindow == null)
        {
            _parentWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)
                ?.MainWindow;
        }

        if (_parentWindow == null) return;

        // Find target control
        Control? targetControl = null;
        if (!string.IsNullOrEmpty(step.TargetControlName))
        {
            targetControl = _parentWindow.FindControl<Control>(step.TargetControlName);
        }

        ShowHighlight(targetControl, step);
    }

    private void ShowHighlight(Control? targetControl, TutorialStep step)
    {
        HideHighlight();

        if (_parentWindow == null) return;

        // Create highlight popup
        _highlightPopup = new Popup
        {
            IsLightDismissEnabled = false,
            Placement = PlacementMode.Pointer
        };

        // Create highlight border with tooltip
        _highlightBorder = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#7AB51D")),
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            MaxWidth = 350,
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Blur = 20,
                Color = Colors.Black,
                OffsetX = 0,
                OffsetY = 4,
                Spread = 0
            })
        };

        var content = new StackPanel { Spacing = 12 };

        // Step counter
        var stepCounter = new TextBlock
        {
            Text = $"Step {_currentStepIndex + 1} of {_activeTutorial?.Steps.Count ?? 0}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#666")),
            FontWeight = FontWeight.SemiBold
        };
        content.Children.Add(stepCounter);

        // Title
        var title = new TextBlock
        {
            Text = step.Title,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#2D2D2D")),
            TextWrapping = TextWrapping.Wrap
        };
        content.Children.Add(title);

        // Description
        var description = new TextBlock
        {
            Text = step.Description,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#666")),
            TextWrapping = TextWrapping.Wrap
        };
        content.Children.Add(description);

        // Navigation buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0)
        };

        if (_currentStepIndex > 0)
        {
            var backButton = new Button
            {
                Content = "← Back",
                Padding = new Thickness(12, 6),
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = new SolidColorBrush(Color.Parse("#7AB51D"))
            };
            backButton.Click += (s, e) => ShowPreviousStep();
            buttonPanel.Children.Add(backButton);
        }

        var skipButton = new Button
        {
            Content = "Skip Tutorial",
            Padding = new Thickness(12, 6),
            Background = new SolidColorBrush(Colors.Transparent),
            Foreground = new SolidColorBrush(Color.Parse("#999"))
        };
        skipButton.Click += (s, e) => EndTutorial();
        buttonPanel.Children.Add(skipButton);

        var nextButton = new Button
        {
            Content = _currentStepIndex >= (_activeTutorial?.Steps.Count ?? 1) - 1 ? "Finish" : "Next →",
            Padding = new Thickness(16, 6),
            Background = new SolidColorBrush(Color.Parse("#7AB51D")),
            Foreground = new SolidColorBrush(Colors.White),
            FontWeight = FontWeight.SemiBold
        };
        nextButton.Click += (s, e) => ShowNextStep();
        buttonPanel.Children.Add(nextButton);

        content.Children.Add(buttonPanel);

        _highlightBorder.Child = content;
        _highlightPopup.Child = _highlightBorder;

        // Position popup
        if (targetControl != null && targetControl.IsVisible)
        {
            _highlightPopup.PlacementTarget = targetControl;
            _highlightPopup.Placement = step.Position switch
            {
                TutorialPosition.Top => PlacementMode.Top,
                TutorialPosition.Bottom => PlacementMode.Bottom,
                TutorialPosition.Left => PlacementMode.Left,
                TutorialPosition.Right => PlacementMode.Right,
                _ => PlacementMode.Bottom
            };
        }
        else
        {
            // Center on screen
            _highlightPopup.PlacementTarget = _parentWindow;
            _highlightPopup.Placement = PlacementMode.Center;
        }

        _highlightPopup.Open();
    }

    private void HideHighlight()
    {
        _highlightPopup?.Close();
        _highlightPopup = null;
        _highlightBorder = null;
    }
}

public class Tutorial
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TutorialStep> Steps { get; set; } = new();
}

public class TutorialStep
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetControlName { get; set; }
    public TutorialPosition Position { get; set; } = TutorialPosition.Bottom;
    public Thickness HighlightPadding { get; set; } = new Thickness(4);
}

public enum TutorialPosition
{
    Top,
    Bottom,
    Left,
    Right,
    Center
}
