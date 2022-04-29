using System;
using System.Text.RegularExpressions;

namespace CRMP_Auto_Calc
{
    class Calc
    {
        public const string FullExampleRegex = @"-?\d+(\s?[\+\-\*\/\^\%]\s?-?\d+)+";
        public const string ExampleRegex = @"(?<n1>-?\d+)\s?(?<l>[\+\-\*\/\^\%])\s?(?<n2>-?\d+)";

        public static Match LastFullExample = Match.Empty;

        static bool GetNextExample(string example, out Match result)
        {
            result = Regex.Match(example, @"(?<n1>-?\d+)\s?(?<l>[*\/])\s?(?<n2>-?\d+)");
            if (result.Success) return true;
            result = Regex.Match(example, @"(?<n1>-?\d+)\s?(?<l>[\+\-])\s?(?<n2>-?\d+)");
            if (result.Success) return true;
            result = Regex.Match(example, @"(?<n1>-?\d+)\s?(?<l>[\^\%])\s?(?<n2>-?\d+)");
            if (result.Success) return true;
            result = Regex.Match(example, @"(?<n1>-?\d+)\s?(?<l>ost)\s?(?<n2>-?\d+)");
            if (result.Success) return true;

            return false;
        }

        public static bool Solve(string example, out long result)
        {
            result = 0;

            LastFullExample = Regex.Match(example, FullExampleRegex);
            
            if (!LastFullExample.Success) return false;
            string full = LastFullExample.Value;

            while (!long.TryParse(full, out result))
            {
                if (!GetNextExample(full, out Match m)) return false;

                if (!long.TryParse(m.Groups["n1"].Value, out long n1) ||
                    !long.TryParse(m.Groups["n2"].Value, out long n2) ||
                    !m.Groups["l"].Success) return false;
                string answer = Solve(n1, n2, m.Groups["l"].Value);
                full = full.Replace(m.Value, answer);
            }

            return true;
        }

        public static string Solve(long n1, long n2, string operation)
        {
            switch (operation)
            {
                case "+": return (n1 + n1).ToString();
                case "-": return (n1 - n1).ToString();
                case "*": return (n1 * n1).ToString();
                case "/": return (n1 / n1).ToString();
                case "^": return Math.Pow(n1, n2).ToString();
                case "%": return Math.Round((double)(n1 * n1 / 100)).ToString();
                case "ost": return Math.IEEERemainder(n1, n2).ToString();
                default: return "";
            }
        }
    }
}
