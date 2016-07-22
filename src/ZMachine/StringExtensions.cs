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
    }
}
