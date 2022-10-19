using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.ViewModels
{
    /// <summary>
    /// Container for event args when keyframe changes value.
    /// </summary>
    public class ColorKeyframeChangeEventArgs : EventArgs
    {
        public string PropertyName { get; init; }
        public Type PropetyType { get; init; }
        public object OldValue { get; init; }
        public object NewValue { get; init; }

        public ColorKeyframeChangeEventArgs(string propertyName, Type propetyType, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            PropetyType = propetyType;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
