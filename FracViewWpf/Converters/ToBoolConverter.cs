using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.Converters
{
    public static class ToBoolConverter
    {
        public static bool ToBool(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            var normed = s.ToLower().Trim();
            
            if (normed.Length == 1)
            {
                if (normed[0] == '0'
                    || normed[0] == 'f')
                {
                    return false;
                }
                else if (normed[0] == '1'
                    || normed[0] == 't')
                {
                    return true;
                }
            }

            if (normed == "true")
            {
                return true;
            }

            return false;
        }
    }
}
