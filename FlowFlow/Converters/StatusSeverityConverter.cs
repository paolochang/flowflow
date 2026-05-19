using FlowFlow.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace FlowFlow.Converters;

public sealed class StatusSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            StatusSeverity.Warning => InfoBarSeverity.Warning,
            StatusSeverity.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
