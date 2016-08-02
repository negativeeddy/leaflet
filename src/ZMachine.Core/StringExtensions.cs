using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZMachine
{
    public static class StringExtensions
    {
        public static void Append(this StringBuilder sb, char value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(value);
            }
        }

        public static string ConcatToString<T>(this IEnumerable<T> values, char separator = ',')
        {
            return values.ConcatToString(separator, x => x.ToString());
        }

        public static string ConcatToString<T>(this IEnumerable<T> values, char separator, Func<T, string> valueFormatter)
        {
            return values.Aggregate(
                new StringBuilder(),
                (sb, val) =>
                {
                    sb.Append(valueFormatter(val));
                    sb.Append(separator);
                    return sb;
                },
                sb => sb.Length == 0 ? string.Empty : sb.Remove(sb.Length - 1, 1).ToString()
                );
        }
    }
}
