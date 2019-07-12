using System;
using System.Text.RegularExpressions;
using Hors.Dict;
using Hors.Models;
using Hors.Utils;

namespace Hors.Recognizers
{
    public class DayOfWeekRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "([usxy])?(D)"; // [в] (следующий/этот/предыдущий) понедельник
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            var date = new AbstractPeriod();
            
            // day of week
            var dayOfWeek = ParserUtils.FindIndex(data.Tokens[match.Groups[2].Index], Keywords.DaysOfWeek()) + 1;
            var userDayOfWeek = (int) userDate.DayOfWeek;
            if (userDayOfWeek == 0) userDayOfWeek = 7; // starts from Monday, not Sunday
            var diff = dayOfWeek - userDayOfWeek;
            
            if (match.Groups[1].Success)
            {
                switch (match.Groups[1].Value)
                {
                    case "y": // "closest next"
                        if (diff < 0) diff += 7;
                        date.Date = userDate.AddDays(diff);
                        break;
                    case "x": // "next"
                        date.Date = userDate.AddDays(diff + 7);
                        break;
                    case "s": // "previous"
                        date.Date = userDate.AddDays(diff - 7);
                        break;
                    case "u": // "current"
                        date.Date = userDate.AddDays(diff);
                        break;
                }
                date.FixDownTo(FixPeriod.Day);
            }
            else
            {
                if (diff < 0) diff += 7;
                date.Date = userDate.AddDays(diff);
                date.Fix(FixPeriod.Day);
            }
            
            // remove and insert
            data.RemoveAndInsert(match.Index, match.Length, date);

            return true;
        }
    }
}