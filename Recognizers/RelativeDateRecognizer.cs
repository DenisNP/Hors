using System;
using System.Text.RegularExpressions;
using Hors.Models;

namespace Hors.Recognizers
{
    public class RelativeDateRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "([usxy])([Ymwd])"; // [в/на] следующей/этой/предыдущей год/месяц/неделе/день
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            var date = new AbstractPeriod();
            var direction = 0;
            
            // relative type
            switch (match.Groups[1].Value)
            {
                case "y":
                case "x":
                    direction = 1; // "next" or "closest next"
                    break;
                case "s":
                    direction = -1;
                    break;
            }
            
            // time period type
            switch (match.Groups[2].Value)
            {
                case "Y":
                    date.Date = userDate.AddYears(direction);
                    date.Fix(FixPeriod.Year);
                    break;
                case "m":
                    date.Date = userDate.AddMonths(direction);
                    date.FixDownTo(FixPeriod.Month);
                    break;
                case "w":
                    date.Date = userDate.AddDays(direction * 7);
                    date.FixDownTo(FixPeriod.Week);
                    break;
                case "d":
                    date.Date = userDate.AddDays(direction);
                    date.FixDownTo(FixPeriod.Day);
                    break;
            }
            
            // remove and insert
            data.RemoveAndInsert(match.Index, match.Length, date);

            return true;
        }
    }
}