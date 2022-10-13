using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FracViewWpf.Converters
{
    [ValueConversion(typeof(bool), typeof(Color))]
    public class BoolToValidationColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if ((bool)value)
                {
                    return Constants.Ui.ValidTextBackgroundColor;
                }
                else
                {
                    return Constants.Ui.InvalidTextBackgroundColor;
                }
            }

            return Constants.Ui.ValidTextBackgroundColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack() of BoolToInvertedBoolConverter is not implemented");
        }
    }
}
