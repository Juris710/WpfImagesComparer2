using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfImagesComparer2
{
    public class HotKeyConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hotKeyText = (string)value;
            var x = Core.Utils.ParseHotKeyText(hotKeyText);
            return new HotKey(x.key, x.modifierKeys);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((HotKey)value).ToString();
        }
    }
}
