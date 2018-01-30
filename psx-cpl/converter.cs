using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace psx_cpl
{
    public class EnumarableToTextConverter : IValueConverter
    {
        public object Convert(
          object value, Type targetType,
          object parameter, CultureInfo culture)
        {
            Console.WriteLine("EnumarableToTextConverter value: " + value);

            if (value is IEnumerable)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var s in value as IEnumerable)
                {
                    sb.AppendLine(s.ToString());
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(
          object value, Type targetType,
          object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
