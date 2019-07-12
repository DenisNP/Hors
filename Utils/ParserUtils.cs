using System;
using System.Collections.Generic;
using Hors.Dict;

namespace Hors
{
    internal static class ParserUtils
    {
        internal static Period GetMin(Period first, Period second)
        {
            return first < second ? first : second;
        }
        
        internal static void FixZeros(List<string> tokens)
        {
            var i = 0;
            while (i < tokens.Count - 1)
            {
                var current = tokens[i];
                var next = tokens[i + 1];

                if (current == "0" && tokens[i + 1].Length == 1 && int.TryParse(tokens[i + 1], out _))
                {
                    // this in zero and number after it, delete zero
                    tokens.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        internal static int FindIndex(string t, IList<string[]> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (Morph.HasOneOfLemmas(t, list[i]))
                    return i;
            }

            return -1;
        }

        internal static int GetYearFromNumber(int n)
        {
            // this is number less than 1000, what year it can be?
                
            if (n >= 70 && n < 100)
            {
                // for numbers from 70 to 99 this will be 19xx year
                return 1900 + n;
            }

            if (n < 1000)
            {
                // for other numbers 20xx or 2xxx year
                return 2000 + n;
            }
            
            return n;
        }

        internal static int GetDayValidForMonth(int year, int month, int day)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            return Math.Max(1, Math.Min(day, daysInMonth));
        }

        internal static int GetDayValidForMonth(DateTime dateTime, int day)
        {
            return GetDayValidForMonth(dateTime.Year, dateTime.Month, day);
        }

        internal static int ExtractIndex(string s)
        {
            if (s.StartsWith("{") && s.EndsWith("}"))
            {
                if (int.TryParse(s.Substring(1, s.Length - 2), out var n))
                {
                    return n;
                }
            }

            return -1;
        }
        
        internal static DateTime DateFromMonthAndDay(int month, int day, DateTime uDate, bool daySpecified = false)
        {
            return new DateTime(uDate.Year, month, GetDayValidForMonth(uDate.Year, month, day));
        }

        internal static TimeSpan SpanFromDayTime(DayTime dayTime)
        {
            switch (dayTime)
            {
                case DayTime.Morning:
                    return new TimeSpan(9, 0, 0);
                case DayTime.Noon:
                    return new TimeSpan(12, 0, 0);
                case DayTime.Evening:
                    return new TimeSpan(19, 0, 0);
                case DayTime.Night:
                    return new TimeSpan(1, 0, 0);
                case DayTime.Day:
                    return new TimeSpan(14, 0, 0);
            }
            
            return new TimeSpan();
        }

        internal static TimeSpan TimeSpanFromPeriod(int number, Period period)
        {
            switch (period)
            {
                case Period.Year:
                    return new TimeSpan(365 * number, 0, 0, 0);
                case Period.Month:
                    return new TimeSpan(30 * number, 0, 0, 0);
                case Period.Week:
                    return new TimeSpan(7 * number, 0, 0, 0);
                case Period.Day:
                    return new TimeSpan(number, 0, 0, 0);
                case Period.Hour:
                    return new TimeSpan(number, 0, 0);
                case Period.Minute:
                    return new TimeSpan(0, number, 0);
            }
            return new TimeSpan();
        }
    }
}