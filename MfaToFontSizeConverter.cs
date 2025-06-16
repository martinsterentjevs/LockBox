using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace LockBox
{
    public class MfaToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string mfa && !string.IsNullOrEmpty(mfa))
            {
                return 36; // Font size for OTP code
            }
            return 18; // Font size for "MFA Not Set"
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
