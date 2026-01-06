using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Dfc.Desktop.Models;

namespace Dfc.Desktop.Controls;

public partial class TutorialAnnotationControl : UserControl
{
    public static readonly StyledProperty<IEnumerable<TutorialAnnotation>?> AnnotationsProperty =
        AvaloniaProperty.Register<TutorialAnnotationControl, IEnumerable<TutorialAnnotation>?>(
            nameof(Annotations));

    public IEnumerable<TutorialAnnotation>? Annotations
    {
        get => GetValue(AnnotationsProperty);
        set => SetValue(AnnotationsProperty, value);
    }

    public TutorialAnnotationControl()
    {
        InitializeComponent();

        // Re-render annotations when the property changes
        AnnotationsProperty.Changed.AddClassHandler<TutorialAnnotationControl>((sender, e) => sender.RenderAnnotations());
        BoundsProperty.Changed.AddClassHandler<TutorialAnnotationControl>((sender, e) => sender.RenderAnnotations());
    }

    private void RenderAnnotations()
    {
        AnnotationCanvas.Children.Clear();

        if (Annotations == null || Bounds.Width == 0 || Bounds.Height == 0)
            return;

        foreach (var annotation in Annotations)
        {
            var element = CreateAnnotationElement(annotation);
            if (element != null)
            {
                AnnotationCanvas.Children.Add(element);
            }
        }
    }

    private Control? CreateAnnotationElement(TutorialAnnotation annotation)
    {
        var color = ParseColor(annotation.Color ?? "#7AB51D");

        return annotation.Type switch
        {
            TutorialAnnotationType.Arrow => CreateArrow(annotation, color),
            TutorialAnnotationType.Highlight => CreateHighlight(annotation, color, false),
            TutorialAnnotationType.CircleHighlight => CreateHighlight(annotation, color, true),
            TutorialAnnotationType.Callout => CreateCallout(annotation, color),
            TutorialAnnotationType.NumberBadge => CreateNumberBadge(annotation, color),
            TutorialAnnotationType.Label => CreateLabel(annotation, color),
            _ => null
        };
    }

    private Control CreateArrow(TutorialAnnotation annotation, Color color)
    {
        var x = annotation.X / 100.0 * Bounds.Width;
        var y = annotation.Y / 100.0 * Bounds.Height;
        var length = (annotation.Length ?? 10) / 100.0 * Bounds.Width;
        var direction = annotation.Direction ?? 0;

        var canvas = new Canvas
        {
            Width = length + 20,
            Height = 20
        };

        // Arrow line
        var line = new Line
        {
            StartPoint = new Point(0, 10),
            EndPoint = new Point(length, 10),
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 3
        };

        // Arrow head
        var arrowHead = new Polygon
        {
            Points = new[]
            {
                new Point(length, 10),
                new Point(length - 10, 5),
                new Point(length - 10, 15)
            },
            Fill = new SolidColorBrush(color)
        };

        canvas.Children.Add(line);
        canvas.Children.Add(arrowHead);

        // Apply rotation
        canvas.RenderTransform = new RotateTransform(direction);
        Canvas.SetLeft(canvas, x);
        Canvas.SetTop(canvas, y);

        if (annotation.Animate)
        {
            canvas.Classes.Add("animated");
        }

        return canvas;
    }

    private Control CreateHighlight(TutorialAnnotation annotation, Color color, bool isCircle)
    {
        var x = annotation.X / 100.0 * Bounds.Width;
        var y = annotation.Y / 100.0 * Bounds.Height;
        var width = (annotation.Width ?? 10) / 100.0 * Bounds.Width;
        var height = (annotation.Height ?? 10) / 100.0 * Bounds.Height;

        var highlightColor = Color.FromArgb(80, color.R, color.G, color.B);
        var borderColor = Color.FromArgb(200, color.R, color.G, color.B);

        Border border;
        if (isCircle)
        {
            var size = Math.Max(width, height);
            border = new Border
            {
                Width = size,
                Height = size,
                CornerRadius = new CornerRadius(size / 2),
                Background = new SolidColorBrush(highlightColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(3)
            };
            Canvas.SetLeft(border, x - size / 2);
            Canvas.SetTop(border, y - size / 2);
        }
        else
        {
            border = new Border
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(highlightColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(3)
            };
            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y);
        }

        if (annotation.Animate)
        {
            border.Classes.Add("animated");
        }

        return border;
    }

    private Control CreateCallout(TutorialAnnotation annotation, Color color)
    {
        var x = annotation.X / 100.0 * Bounds.Width;
        var y = annotation.Y / 100.0 * Bounds.Height;

        var callout = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 2,
                Blur = 8,
                Color = Color.FromArgb(60, 0, 0, 0)
            }),
            Child = new TextBlock
            {
                Text = annotation.Text ?? string.Empty,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextWrapping = TextWrapping.NoWrap
            }
        };

        Canvas.SetLeft(callout, x);
        Canvas.SetTop(callout, y);

        if (annotation.Animate)
        {
            callout.Classes.Add("animated");
        }

        return callout;
    }

    private Control CreateNumberBadge(TutorialAnnotation annotation, Color color)
    {
        var x = annotation.X / 100.0 * Bounds.Width;
        var y = annotation.Y / 100.0 * Bounds.Height;

        var badge = new Border
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(color),
            BorderBrush = new SolidColorBrush(Colors.White),
            BorderThickness = new Thickness(3),
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 2,
                Blur = 8,
                Color = Color.FromArgb(80, 0, 0, 0)
            }),
            Child = new TextBlock
            {
                Text = annotation.Text ?? "1",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        Canvas.SetLeft(badge, x - 16);
        Canvas.SetTop(badge, y - 16);

        if (annotation.Animate)
        {
            badge.Classes.Add("animated");
        }

        return badge;
    }

    private Control CreateLabel(TutorialAnnotation annotation, Color color)
    {
        var x = annotation.X / 100.0 * Bounds.Width;
        var y = annotation.Y / 100.0 * Bounds.Height;

        var label = new TextBlock
        {
            Text = annotation.Text ?? string.Empty,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(color),
            Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            Padding = new Thickness(8, 4)
        };

        Canvas.SetLeft(label, x);
        Canvas.SetTop(label, y);

        if (annotation.Animate)
        {
            label.Classes.Add("animated");
        }

        return label;
    }

    private static Color ParseColor(string hexColor)
    {
        try
        {
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            if (hexColor.Length == 6)
            {
                var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                return Color.FromRgb(r, g, b);
            }
        }
        catch
        {
            // Fall back to default color
        }

        return Color.FromRgb(122, 181, 29); // Default Desktop Food Cost green
    }
}
