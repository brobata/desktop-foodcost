using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Controls.Documents;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Freecost.Desktop.Converters;

/// <summary>
/// Highlights search terms in text with yellow background
/// </summary>
public class SearchHighlightConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not string text || values[1] is not string searchTerm)
            return values[0]?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(text))
            return text;

        var inlineCollection = new InlineCollection();
        var startIndex = 0;
        var lowerText = text.ToLower();
        var lowerSearch = searchTerm.ToLower();

        while (startIndex < text.Length)
        {
            var index = lowerText.IndexOf(lowerSearch, startIndex, StringComparison.Ordinal);

            if (index == -1)
            {
                // Add remaining text
                inlineCollection.Add(new Run(text.Substring(startIndex)));
                break;
            }

            // Add text before match
            if (index > startIndex)
            {
                inlineCollection.Add(new Run(text.Substring(startIndex, index - startIndex)));
            }

            // Add highlighted match
            var matchedText = text.Substring(index, searchTerm.Length);
            var highlightedRun = new Run(matchedText)
            {
                Background = new SolidColorBrush(Color.Parse("#FFEB3B")), // Yellow
                Foreground = new SolidColorBrush(Color.Parse("#000000"))  // Black text
            };
            inlineCollection.Add(highlightedRun);

            startIndex = index + searchTerm.Length;
        }

        return inlineCollection;
    }
}
