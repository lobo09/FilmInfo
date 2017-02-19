using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FilmInfo.Converter
{
    public class RatingToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && value is double)
            {
                string lowColorPreset = "#FFBE0303";
                string midColorPreset = "#FFFBFE41";
                string hiColorPreset = "#FF18F900";
                string lowerColor = "#FF000000";
                string higherColor = "#FFFFFFFF";
                double factor = 1;
           
                if ((double)value == 0.0)
                {
                    return lowColorPreset;
                }
                if((double)value == 5.0)
                {
                    return midColorPreset;
                }
                if ((double)value == 10.0)
                {
                    return hiColorPreset;
                }
                if ((double)value < 5.0)
                {
                    lowerColor = lowColorPreset;
                    higherColor = midColorPreset;
                    factor = (double)value / 5;

                }
                if ((double)value > 5.0)
                {
                    lowerColor = midColorPreset;
                    higherColor = hiColorPreset;
                    factor = ((double)value-5) / 5;
                }

                int lowR = System.Convert.ToInt32(lowerColor.Substring(3, 2),16);
                int lowG = System.Convert.ToInt32(lowerColor.Substring(5, 2), 16);
                int lowB = System.Convert.ToInt32(lowerColor.Substring(7, 2), 16);
                int hiR = System.Convert.ToInt32(higherColor.Substring(3, 2), 16);
                int hiG = System.Convert.ToInt32(higherColor.Substring(5, 2), 16);
                int hiB = System.Convert.ToInt32(higherColor.Substring(7, 2), 16);

                int calculatedR = (int)(lowR + ((hiR - lowR) * factor));//TODO
                int calculatedG = (int)(lowG + ((hiG - lowG) * factor));//TODO
                int calculatedB = (int)(lowB + ((hiB - lowB) * factor));//TODO


                string finalR = String.Format("{0:X2}", calculatedR);
                string finalG = String.Format("{0:X2}", calculatedG);
                string finalB = String.Format("{0:X2}", calculatedB);

                string result = $"#FF{finalR}{finalG}{finalB}";
                return result;
            }
            else
            {
                return "#FF000000";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
