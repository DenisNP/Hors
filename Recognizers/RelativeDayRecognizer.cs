using System;
using System.Text.RegularExpressions;
using Hors.Models;

namespace Hors.Recognizers
{
    public class RelativeDayRecognizer : Recognizer
    {
        internal override string GetRegexPattern()
        {
            return "[2-6]"; // позавчера, вчера, сегодня, завтра, послезавтра
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            if (!int.TryParse(match.Value, out var relativeDay)) return false;
            relativeDay -= 4;
            
            // create date
            var date = new AbstractPeriod
            {
                Date = userDate.AddDays(relativeDay)
            };
            date.FixDownTo(FixPeriod.Day);
            
            // remove and insert
            RemoveRange(data, match.Index, match.Length);
            InsertDates(data, match.Index, date);

            return true;
        }
    }
}