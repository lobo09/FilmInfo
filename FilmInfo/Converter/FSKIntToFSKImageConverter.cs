using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FilmInfo.Converter
{
    public class FSKIntToFSKImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is int && value != null)
            {
                switch (value)
                {
                    case 0:
                        return "/Resources/Images/FSK/FSK0.png";
                    case 6:
                        return "/Resources/Images/FSK/FSK6.png";
                    case 12:
                        return "/Resources/Images/FSK/FSK12.png";
                    case 16:
                        return "/Resources/Images/FSK/FSK16.png";
                    case 18:
                        return "/Resources/Images/FSK/FSK18.png";
                    default:
                        return "/Resources/Images/FSK/FSKUnknown.png";
                }
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
