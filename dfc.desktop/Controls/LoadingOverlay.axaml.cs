using Avalonia;
using Avalonia.Controls;

namespace Dfc.Desktop.Controls;

public partial class LoadingOverlay : UserControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<LoadingOverlay, bool>(nameof(IsLoading), false);

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<LoadingOverlay, string>(nameof(Message), "Loading...");

    public static readonly StyledProperty<LoadingStyle> LoadingStyleProperty =
        AvaloniaProperty.Register<LoadingOverlay, LoadingStyle>(nameof(LoadingStyle), LoadingStyle.Spinner);

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public LoadingStyle LoadingStyle
    {
        get => GetValue(LoadingStyleProperty);
        set => SetValue(LoadingStyleProperty, value);
    }

    public LoadingOverlay()
    {
        InitializeComponent();
    }
}
