using System;
using System.Globalization;
using System.Windows.Data;

namespace EDShoppingList
{
    public class NoDataValueConverter : IValueConverter
    {
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string noData = "No data";
            string format = "{0}";
            if (parameter != null && parameter is string)
            {
                string strParam = (string)parameter;
                if (strParam.Contains("|"))
                {
                    var parms = strParam.Split('|');
                    format = parms[0];
                    noData = parms[1];
                }
            }
            if (value == null)
            {
                return noData;
            }
            if (value is Nullable)
            {
                return (bool)GetPropValue(value, "HasValue") ? string.Format(format, value) : noData;
            }
            return string.Format(format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
