using System;
using System.Text.RegularExpressions;

namespace CRMP_Auto_Calc.Models
{
    class ChatLine
    {
        public const string colorsRegex = @"\{(?<color>[\w\d]{6})\}";
        public const string timeRegex = @"^\[(\d\d):(\d\d):(\d\d)\]";

        public string message;
        public DateTime Time
        {
            get
            {
                Match match = Regex.Match(message, timeRegex);
                if (match.Success)
                {
                    if (match.Groups.Count == 3)
                    {
                        if (int.TryParse(match.Groups[0].Value, out int h) &&
                            int.TryParse(match.Groups[1].Value, out int m) &&
                            int.TryParse(match.Groups[2].Value, out int s))
                        {
                            return new DateTime(0, 0, 0, h, m, s);
                        }
                    }
                }
                return new DateTime(0);
            }
        }

        public ChatLine(string message)
        {
            this.message = message;
        }

        public ChatLine WithoutDate() => new ChatLine(Regex.Replace(message, timeRegex, ""));
        public ChatLine WithoutColors() => new ChatLine(Regex.Replace(message, colorsRegex, ""));
    }
}
