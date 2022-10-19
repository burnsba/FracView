using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FracViewWpf.Constants
{
    /// <summary>
    /// Code reference to constants used in the application.
    /// </summary>
    public class Ui
    {
        private static Color _validTextBackgroundColor = Color.FromArgb(255, 255, 191, 191);

        /// <summary>
        /// Gets the standard background color for textboxes that do not contain invalid text.
        /// </summary>
        public static Color ValidTextBackgroundColor => Colors.White;

        /// <summary>
        /// Gets the standard background color for textboxes that contain invalid text.
        /// </summary>
        public static Color InvalidTextBackgroundColor => _validTextBackgroundColor;
    }
}
