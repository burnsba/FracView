using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FracViewWpf.Constants
{
    public class Ui
    {
        private static Color _validTextBackgroundColor = Color.FromArgb(255, 255, 191, 191);

        public static Color ValidTextBackgroundColor => Colors.White;

        public static Color InvalidTextBackgroundColor => _validTextBackgroundColor;
    }
}
