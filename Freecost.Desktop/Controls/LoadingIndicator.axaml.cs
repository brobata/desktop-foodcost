using Avalonia;
using Avalonia.Controls;

namespace Freecost.Desktop.Controls;

public partial class LoadingIndicator : UserControl
{
    public static readonly StyledProperty<LoadingStyle> StyleProperty =
        AvaloniaProperty.Register<LoadingIndicator, LoadingStyle>(nameof(Style), LoadingStyle.Spinner);

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<LoadingIndicator, string>(nameof(Message), "Loading...");

    public static readonly StyledProperty<bool> ShowMessageProperty =
        AvaloniaProperty.Register<LoadingIndicator, bool>(nameof(ShowMessage), false);

    public LoadingStyle Style
    {
        get => GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool ShowMessage
    {
        get => GetValue(ShowMessageProperty);
        set => SetValue(ShowMessageProperty, value);
    }

    public LoadingIndicator()
    {
        InitializeComponent();

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StyleProperty)
        {
            UpdateStyle();
        }
        else if (e.Property == MessageProperty)
        {
            UpdateMessage();
        }
        else if (e.Property == ShowMessageProperty)
        {
            UpdateMessageVisibility();
        }
    }

    private void UpdateStyle()
    {
        var spinnerStyle = this.FindControl<Border>("SpinnerStyle");
        var dotsStyle = this.FindControl<StackPanel>("DotsStyle");

        if (spinnerStyle != null && dotsStyle != null)
        {
            spinnerStyle.IsVisible = Style == LoadingStyle.Spinner;
            dotsStyle.IsVisible = Style == LoadingStyle.Dots;
        }
    }

    private void UpdateMessage()
    {
        var messageText = this.FindControl<TextBlock>("MessageText");
        if (messageText != null)
        {
            messageText.Text = Message;
        }
    }

    private void UpdateMessageVisibility()
    {
        var messageText = this.FindControl<TextBlock>("MessageText");
        if (messageText != null)
        {
            messageText.IsVisible = ShowMessage;
        }
    }
}

public enum LoadingStyle
{
    Spinner,
    Dots
}
