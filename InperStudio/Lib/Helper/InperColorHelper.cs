using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InperStudio.Lib.Helper
{
    public class InperColorHelper
    {
        public static List<SolidColorBrush> SCBrushes { get; } = new List<SolidColorBrush>()
        {
            Brushes.LimeGreen,
            Brushes.Chocolate,
            Brushes.DarkOliveGreen,
            Brushes.MediumPurple,
            Brushes.HotPink,
            Brushes.Red,
            Brushes.Cyan,
            Brushes.LightPink,
            Brushes.DarkKhaki
        };
        public static readonly List<string> ColorPresetList = new List<string>
        {
            "#f44336",
            "#e91e63",
            "#9c27b0",
            "#673ab7",
            "#3f51b5",
            "#2196f3",
            "#03a9f4",
            "#00bcd4",
            "#009688",

            "#4caf50",
            "#8bc34a",
            "#cddc39",
            "#ffeb3b",
            "#ffc107",
            "#ff9800",
            "#ff5722",
            "#795548",
            "#9e9e9e"
        };
        public static Color WavelengthToRGB(int Wavelength)
        {
            double Gamma = 0.8;
            int IntensityMax = 215;

            double Blue;
            double Green;
            double Red;
            double factor;

            if (Wavelength >= 380 && Wavelength <= 439)
            {
                Red = -(Wavelength - 440d) / (440d - 380d);
                Green = 0.0;
                Blue = 1.0;
            }
            else if (Wavelength >= 440 && Wavelength <= 489)
            {
                Red = 0.0;
                Green = (Wavelength - 440d) / (490d - 440d);
                Blue = 1.0;
            }
            else if (Wavelength >= 490 && Wavelength <= 509)
            {
                Red = 0.0;
                Green = 1.0;
                Blue = -(Wavelength - 510d) / (510d - 490d);
            }
            else if (Wavelength >= 510 && Wavelength <= 579)
            {
                Red = (Wavelength - 510d) / (580d - 510d);
                Green = 1.0;
                Blue = 0.0;
            }
            else if (Wavelength >= 580 && Wavelength <= 644)
            {
                Red = 1.0;
                Green = -(Wavelength - 645d) / (645d - 580d);
                Blue = 0.0;
            }
            else if (Wavelength >= 645 && Wavelength <= 780)
            {
                Red = 1.0;
                Green = 0.0;
                Blue = 0.0;
            }
            else
            {
                Red = 0.0;
                Green = 0.0;
                Blue = 0.0;
            }

            if (Wavelength >= 350 && Wavelength <= 419)
            {
                factor = 0.3 + 0.7 * (Wavelength - 380d) / (420d - 380d);
            }
            else if (Wavelength >= 420 && Wavelength <= 700)
            {
                factor = 1.0;
            }
            else if (Wavelength >= 701 && Wavelength <= 780)
            {
                factor = 0.3 + 0.7 * (780d - Wavelength) / (780d - 700d);
            }
            else
            {
                factor = 0.0;
            }

            byte R = Adjust(Red, factor, IntensityMax, Gamma);
            byte G = Adjust(Green, factor, IntensityMax, Gamma);
            byte B = Adjust(Blue, factor, IntensityMax, Gamma);

            return Color.FromRgb(R, G, B);
        }
        private static byte Adjust(double Color, double Factor, int IntensityMax, double Gamma)
        {
            if (Color == 0.0)
            {
                return 0;
            }
            else
            {
                return (byte)Math.Round(IntensityMax * Math.Pow(Color * Factor, Gamma));
            }

        }
        public static readonly List<string> HotkeysList = new List<string>
        {
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12"
        };
    }
}
