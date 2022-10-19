using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FracViewWpf.Converters;

namespace FracViewWpf.Controls
{
    /// <summary>
    /// Very basic color picker control.
    /// The current color is shown above.
    /// A single textbox allows entry for the color.
    /// The <see cref="ColorConverters.WindowsMedia.FromCss"/> method is used to parse text.
    /// If the text can not be parsed, the control will set <see cref="IsValid"/>to false.
    /// Picking a color using the mouse is not supported.
    /// </summary>
    public partial class SimpleColorPicker : UserControl
    {
        /* The following two fields are used to initialize the text and color
         * during/after constructor.
         */
        private bool _initDone = false;
        private bool _useInitText = true;

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(SimpleColorPicker),
                new PropertyMetadata(Colors.Black, OnSelectedColorPropertyChange));

        /// <summary>
        /// Gets or sets the currently selected color.
        /// </summary>
        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty ValidationColorProperty =
            DependencyProperty.Register(nameof(ValidationColor), typeof(Color), typeof(SimpleColorPicker),
                new PropertyMetadata(Constants.Ui.ValidTextBackgroundColor));

        /// <summary>
        /// Gets or sets the color used when the textbox contains a valid color.
        /// </summary>
        public Color ValidationColor
        {
            get => (Color)GetValue(ValidationColorProperty);
            set => SetValue(ValidationColorProperty, value);
        }

        public static readonly DependencyProperty SelectedColorTextProperty =
            DependencyProperty.Register(nameof(SelectedColorText), typeof(string), typeof(SimpleColorPicker),
                new PropertyMetadata(
                    ColorConverters.WindowsMedia.ToHexSeven(Colors.Black).ToString().ToLower(),
                    OnSelectedColorTextPropertyChange));

        /// <summary>
        /// Gets or sets the text bound to the currently selected color.
        /// </summary>
        public string SelectedColorText
        {
            get => (string)GetValue(SelectedColorTextProperty);
            set => SetValue(SelectedColorTextProperty, value);
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(SimpleColorPicker),
                new PropertyMetadata(true, OnIsValidPropertyChange));

        /// <summary>
        /// Gets or sets a value indicating if the current text parses into a color.
        /// </summary>
        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public SimpleColorPicker()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            _initDone = true;

            base.OnInitialized(e);
        }

        private static void OnSelectedColorPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var scp = d as SimpleColorPicker;
            if (object.ReferenceEquals(null, scp))
            {
                return;
            }

            if (scp._useInitText)
            {
                scp.SelectedColorText = ColorConverters.WindowsMedia.ToHexSeven(scp.SelectedColor);
            }
        }

        private static void OnSelectedColorTextPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
        }

        private static void OnIsValidPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
        }

        private void UpdateValue(Color c, string displayText)
        {
            SelectedColor = c;
            SelectedColorText = displayText;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_initDone)
            {
                return;
            }

            _useInitText = false;

            var text = ((TextBox)e.Source).Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(text) || text.Length < 3)
            {
                if (IsValid)
                {
                    IsValid = false;
                }

                return;
            }

            if (text.StartsWith("#") && text.Length > 6 || !text.StartsWith("#") && text.Length > 2)
            {
                try
                {
                    var drawingColor = ColorConverters.WindowsMedia.FromCss(text);
                    var result = Color.FromArgb(255, drawingColor.R, drawingColor.G, drawingColor.B);
                    UpdateValue(result, text);
                    e.Handled = true;
                    if (!IsValid)
                    {
                        IsValid = true;
                    }
                    return;
                }
                catch
                {
                }
            }

            if (IsValid)
            {
                IsValid = false;
            }
        }
    }
}
