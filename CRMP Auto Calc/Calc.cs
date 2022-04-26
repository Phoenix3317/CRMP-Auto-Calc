using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static System.ConsoleColor;
using static Phoenix3317.ExtendedConsole.ExConsole;

namespace CRMP_Auto_Calc
{
    class Calc
    {
        public const string FullExampleRegex = @"-?\d+(\s?[\+\-\*\/\^\%]\s?-?\d+)+";
        public const string ExampleRegex = @"(?<n1>-?\d+)\s?(?<l>[\+\-\*\/\^\%])\s?(?<n2>-?\d+)";

        public static Match LastFullExample = Match.Empty;

        public static bool Solve(string example, out long result)
        {
            result = 0;

            LastFullExample = Regex.Match(example, FullExampleRegex);
            
            if (!LastFullExample.Success) return false;
            string full = LastFullExample.Value;

            while (!long.TryParse(full, out result))
            {
                Match m = Regex.Match(full, ExampleRegex);
                if (!m.Success) return false;

                if (!long.TryParse(m.Groups["n1"].Value, out long n1) ||
                    !long.TryParse(m.Groups["n2"].Value, out long n2) ||
                    !m.Groups["l"].Success) return false;
                long answer = Solve(n1, n2, m.Groups["l"].Value);
                full = full.Replace(m.Value, answer.ToString());
            }

            return true;
        }

        public static long Solve(long n1, long n2, string operation)
        {
            switch (operation)
            {
                case "+": return n1 + n2;
                case "-": return n1 - n2;
                case "*": return n1 * n2;
                case "/": return n1 / n2;
                case "^": return n1 ^ n2;
                case "%": return n1 % n2;
                default: return 0;
            }
        }
    }
}
