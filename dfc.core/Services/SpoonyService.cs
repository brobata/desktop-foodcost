using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dfc.Core.Services;

public interface ISpoonyService
{
    event EventHandler<SpoonyMessageEventArgs>? SpoonyMessageRequested;
    void TriggerHelp(SpoonyHelpContext context);
    void DismissSpoony();
    bool IsSpoonyEnabled { get; set; }
}

public class SpoonyService : ISpoonyService, INotifyPropertyChanged
{
    public event EventHandler<SpoonyMessageEventArgs>? SpoonyMessageRequested;
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isSpoonyEnabled = false;
    public bool IsSpoonyEnabled
    {
        get => _isSpoonyEnabled;
        set
        {
            if (_isSpoonyEnabled != value)
            {
                _isSpoonyEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void TriggerHelp(SpoonyHelpContext context)
    {
        if (!IsSpoonyEnabled) return;

        var message = GetMessageForContext(context);
        SpoonyMessageRequested?.Invoke(this, new SpoonyMessageEventArgs(message, context));
    }

    public void DismissSpoony()
    {
        // Could track dismissals to reduce frequency
    }

    private SpoonyMessage GetMessageForContext(SpoonyHelpContext context)
    {
        return context switch
        {
            SpoonyHelpContext.Welcome => new SpoonyMessage
            {
                Title = "Woof! I'm Spoony!",
                Message = "I'm here to help you navigate Desktop Food Cost. Click on me anytime if you need assistance!",
                Mood = SpoonyMood.Happy
            },
            SpoonyHelpContext.FirstIngredient => new SpoonyMessage
            {
                Title = "Adding Your First Ingredient",
                Message = "Great start! Add ingredients here and I'll help you track costs. Don't forget to add suppliers for accurate pricing!",
                Mood = SpoonyMood.Excited
            },
            SpoonyHelpContext.FirstRecipe => new SpoonyMessage
            {
                Title = "Creating Your First Recipe",
                Message = "Recipes are where the magic happens! Add your ingredients, set portions, and I'll calculate all the costs for you.",
                Mood = SpoonyMood.Curious
            },
            SpoonyHelpContext.FirstEntree => new SpoonyMessage
            {
                Title = "Building Your First Plate",
                Message = "Plates combine recipes and ingredients. Perfect for menu planning and final dish costing!",
                Mood = SpoonyMood.Happy
            },
            SpoonyHelpContext.CostAnalysis => new SpoonyMessage
            {
                Title = "Cost Analysis Tips",
                Message = "Keep an eye on cost trends! Regular price updates help you catch expensive ingredient changes early.",
                Mood = SpoonyMood.Alert
            },
            SpoonyHelpContext.IdleHelp => new SpoonyMessage
            {
                Title = "Need a Paw?",
                Message = "I noticed you've been here a while. Need help finding something? Just click on me!",
                Mood = SpoonyMood.Curious
            },
            _ => new SpoonyMessage
            {
                Title = "Woof!",
                Message = "I'm here to help! Click the Help button for tutorials and guides.",
                Mood = SpoonyMood.Happy
            }
        };
    }
}

public class SpoonyMessageEventArgs : EventArgs
{
    public SpoonyMessage Message { get; }
    public SpoonyHelpContext Context { get; }

    public SpoonyMessageEventArgs(SpoonyMessage message, SpoonyHelpContext context)
    {
        Message = message;
        Context = context;
    }
}

public class SpoonyMessage
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SpoonyMood Mood { get; set; } = SpoonyMood.Happy;
}

public enum SpoonyMood
{
    Happy,      // Neutral friendly state
    Excited,    // Celebrating user success
    Curious,    // Engaged and interested
    Alert,      // Important information
    Sleeping    // Minimized/idle state
}

public enum SpoonyHelpContext
{
    Welcome,
    FirstIngredient,
    FirstRecipe,
    FirstEntree,
    CostAnalysis,
    IdleHelp,
    General
}
