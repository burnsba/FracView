using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracViewWpf.Converters
{
    public static class MathString
    {
        /// <summary>
        /// Very loose support for math operations.
        /// Operator precedence is not gauranteed.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool DecimalTryParseMathString(string s, out decimal result, out bool hasMath)
        {
            List<decimal> aggregate = new List<decimal>();

            result = 0m;
            hasMath = false;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (s.IndexOf('*') > -1)
            {
                var sections = s.Split('*', StringSplitOptions.RemoveEmptyEntries);
                if (!sections.Any())
                {
                    return false;
                }

                foreach (var section in sections)
                {
                    decimal d;
                    if (!DecimalTryParseMathString(section, out d, out bool _))
                    {
                        return false;
                    }

                    aggregate.Add(d);
                }

                hasMath = true;
                result = aggregate.Aggregate((x, y) => x * y);
                return true;
            }
            else if (s.IndexOf('/') > -1)
            {
                var sections = s.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (!sections.Any())
                {
                    return false;
                }

                foreach (var section in sections)
                {
                    decimal d;
                    if (!DecimalTryParseMathString(section, out d, out bool _))
                    {
                        return false;
                    }

                    // first value can be zero, but anything after is a divide by zero error.
                    if (aggregate.Any() && d == 0)
                    {
                        return false;
                    }

                    aggregate.Add(d);
                }

                hasMath = true;
                result = aggregate.Aggregate((x, y) => x / y);
                return true;
            }
            else if (s.IndexOf('+') > -1)
            {
                var sections = s.Split('+', StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length < 2)
                {
                    // could be leading positive sign
                }
                else
                {
                    foreach (var section in sections)
                    {
                        decimal d;
                        if (!DecimalTryParseMathString(section, out d, out bool _))
                        {
                            return false;
                        }

                        aggregate.Add(d);
                    }

                    hasMath = true;
                    result = aggregate.Aggregate((x, y) => x + y);
                    return true;
                }
            }
            else if (s.IndexOf('-') > -1)
            {
                var sections = s.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length < 2)
                {
                    // could be negative number
                }
                else
                {
                    foreach (var section in sections)
                    {
                        decimal d;
                        if (!DecimalTryParseMathString(section, out d, out bool _))
                        {
                            return false;
                        }

                        aggregate.Add(d);
                    }

                    hasMath = true;
                    result = aggregate.Aggregate((x, y) => x + y);
                    return true;
                }
            }

            var parseResult = decimal.TryParse(s, out decimal parsed);
            if (parseResult)
            {
                result = parsed;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Very loose support for math operations.
        /// Operator precedence is not gauranteed.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IntTryParseMathString(string s, out int result, out bool hasMath)
        {
            List<int> aggregate = new List<int>();

            hasMath = false;
            result = 0;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (s.IndexOf('*') > -1)
            {
                var sections = s.Split('*', StringSplitOptions.RemoveEmptyEntries);
                if (!sections.Any())
                {
                    return false;
                }

                foreach (var section in sections)
                {
                    int d;
                    if (!IntTryParseMathString(section, out d, out bool _))
                    {
                        return false;
                    }

                    aggregate.Add(d);
                }

                hasMath = true;
                result = aggregate.Aggregate((x, y) => x * y);
                return true;
            }
            else if (s.IndexOf('/') > -1)
            {
                var sections = s.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (!sections.Any())
                {
                    return false;
                }

                foreach (var section in sections)
                {
                    int d;
                    if (!IntTryParseMathString(section, out d, out bool _))
                    {
                        return false;
                    }

                    // first value can be zero, but anything after is a divide by zero error.
                    if (aggregate.Any() && d == 0)
                    {
                        return false;
                    }

                    hasMath = true;
                    aggregate.Add(d);
                }

                result = aggregate.Aggregate((x, y) => x / y);
                return true;
            }
            else if (s.IndexOf('+') > -1)
            {
                var sections = s.Split('+', StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length < 2)
                {
                    // could be positive sign
                    // fallthrough
                }
                else
                {
                    foreach (var section in sections)
                    {
                        int d;
                        if (!IntTryParseMathString(section, out d, out bool _))
                        {
                            return false;
                        }

                        aggregate.Add(d);
                    }

                    hasMath = true;
                    result = aggregate.Aggregate((x, y) => x + y);
                    return true;
                }

            }
            else if (s.IndexOf('-') > -1)
            {
                var sections = s.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length < 2)
                {
                    // could be negative sign
                    // fallthrough
                }
                else
                {
                    foreach (var section in sections)
                    {
                        int d;
                        if (!IntTryParseMathString(section, out d, out bool _))
                        {
                            return false;
                        }

                        aggregate.Add(d);
                    }

                    hasMath = true;
                    result = aggregate.Aggregate((x, y) => x + y);
                    return true;
                }
            }

            var parseResult = int.TryParse(s, out int parsed);
            if (parseResult)
            {
                result = parsed;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
