using Hors.Dict;
using Hors.Models;

namespace Hors.Utils
{
    internal static class ParserExtractors
    {
        internal static string CreatePatternFromToken(string token)
        {
            var t = token.ToLower().Replace("[^0-9а-яё-]", "").Trim();

            if (Morph.HasOneOfLemmas(t, Keywords.Year)) return "Y";
            if (Morph.HasOneOfLemmas(t, Keywords.Months().ToArray())) return "M";
            if (Morph.HasOneOfLemmas(t, Keywords.DaysOfWeek().ToArray())) return "D";
            if (Morph.HasOneOfLemmas(t, Keywords.PreviousPostfix)) return "b";
            if (Morph.HasOneOfLemmas(t, Keywords.AfterPostfix)) return "l";
            if (Morph.HasOneOfLemmas(t, Keywords.After)) return "i";
            if (Morph.HasOneOfLemmas(t, Keywords.Holiday)) return "W";
            
            var p = PeriodFromToken(t);
            switch (p)
            {
                case Period.Minute:
                    return "e";
                case Period.Hour:
                    return "h";
                case Period.Day:
                    return "d";
                case Period.Week:
                    return "w";
                case Period.Month:
                    return "m";
            }

            var r = RelativeModeFromToken(t);
            switch (r)
            {
                case RelativeMode.Previous:
                    return "s";
                case RelativeMode.Current:
                    return "u";
                case RelativeMode.CurrentNext:
                    return "y";
                case RelativeMode.Next:
                    return "x";
            }

            var n = NeighbourDaysFromToken(t);
            if (n > int.MinValue)
            {
                return (n + 4).ToString();
            }

            var d = DaytimeFromToken(t);
            switch (d)
            {
                case DayTime.Morning:
                    return "r";
                case DayTime.Noon:
                    return "n";
                case DayTime.Day:
                    return "a";
                case DayTime.Evening:
                    return "v";
                case DayTime.Night:
                    return "g";
            }

            var pt = PartTimeFromToken(t);
            switch (pt)
            {
                case PartTime.Quarter:
                    return "Q";
                case PartTime.Half:
                    return "H";
            }
            
            if (int.TryParse(t, out var c))
            {
                if (c < 0 || c > 9999) return "_";
                if (c > 1900) return "1";
                return "0";
            }
            
            if (Morph.HasOneOfLemmas(t, Keywords.TimeFrom)) return "f";
            if (Morph.HasOneOfLemmas(t, Keywords.TimeTo)) return "t";
            if (Morph.HasOneOfLemmas(t, Keywords.TimeOn)) return "o";
            if (Morph.HasOneOfLemmas(t, Keywords.DayInMonth)) return "#";
            
            if (t == "и") return "N";

            return "_";
        }
        
        private static PartTime PartTimeFromToken(string t)
        {
            if (Morph.HasOneOfLemmas(t, Keywords.Quarter)) return PartTime.Quarter;
            if (Morph.HasOneOfLemmas(t, Keywords.Half)) return PartTime.Half;

            return PartTime.None;
        }

        private static DayTime DaytimeFromToken(string t)
        {
            if (Morph.HasOneOfLemmas(t, Keywords.Noon)) return DayTime.Noon;
            if (Morph.HasOneOfLemmas(t, Keywords.Morning)) return DayTime.Morning;
            if (Morph.HasOneOfLemmas(t, Keywords.Evening)) return DayTime.Evening;
            if (Morph.HasOneOfLemmas(t, Keywords.Night)) return DayTime.Night;
            if (Morph.HasOneOfLemmas(t, Keywords.DaytimeDay)) return DayTime.Day;

            return DayTime.None;
        }

        private static Period PeriodFromToken(string t)
        {
            if (Morph.HasOneOfLemmas(t, Keywords.Year)) return Period.Year;
            if (Morph.HasOneOfLemmas(t, Keywords.Month)) return Period.Month;
            if (Morph.HasOneOfLemmas(t, Keywords.Week)) return Period.Week;
            if (Morph.HasOneOfLemmas(t, Keywords.Day)) return Period.Day;
            if (Morph.HasOneOfLemmas(t, Keywords.Hour)) return Period.Hour;
            if (Morph.HasOneOfLemmas(t, Keywords.Minute)) return Period.Minute;

            return Period.None;
        }

        private static int NeighbourDaysFromToken(string t)
        {
            if (Morph.HasOneOfLemmas(t, Keywords.Tomorrow)) return 1;
            if (Morph.HasOneOfLemmas(t, Keywords.Today)) return 0;
            if (Morph.HasOneOfLemmas(t, Keywords.AfterTomorrow)) return 2;
            if (Morph.HasOneOfLemmas(t, Keywords.Yesterday)) return -1;
            if (Morph.HasOneOfLemmas(t, Keywords.BeforeYesterday)) return -2;
            
            return int.MinValue;
        }

        internal static RelativeMode RelativeModeFromToken(string t)
        {
            if (Morph.HasOneOfLemmas(t, Keywords.Current)) return RelativeMode.Current;
            if (Morph.HasOneOfLemmas(t, Keywords.Next)) return RelativeMode.Next;
            if (Morph.HasOneOfLemmas(t, Keywords.Previous)) return RelativeMode.Previous;
            if (Morph.HasOneOfLemmas(t, Keywords.CurrentNext)) return RelativeMode.CurrentNext;

            return RelativeMode.None;
        }
    }
}