using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.ViewModels
{
    public class ColorKeyframeChangeEventArgs : EventArgs
    {
        public string PropertyName { get; set; }
        public Type PropetyType { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
