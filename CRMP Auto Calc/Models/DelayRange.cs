using System.Text.RegularExpressions;

namespace CRMP_Auto_Calc.Models
{
    class DelayRange
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public DelayRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public DelayRange(int max)
        {
            Min = 0;
            Max = max;
        }

        public static bool TryParse(string input, out DelayRange result)
        {
            result = null;

            Match m = Regex.Match(input, @"(\d+)\s*[-\s]\s*(\d+)");
            if (!m.Success) return false;
            if (!int.TryParse(m.Groups[1].Value, out int min) ||
                !int.TryParse(m.Groups[2].Value, out int max)) return false;
            if (min > max) return false;
            result = new DelayRange(min, max);
            return true;
        }
    }
}
