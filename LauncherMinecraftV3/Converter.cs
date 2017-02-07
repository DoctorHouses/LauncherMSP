using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LauncherMinecraftV3
{
    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }


    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            DateTime time = DateTime.ParseExact((string) value, "yyyy-MM-ddTHH:mm:ssZ",
                CultureInfo.InvariantCulture);
            return time.ToString(CultureInfo.InvariantCulture);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
