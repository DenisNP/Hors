using System;
using System.Text.RegularExpressions;
using Hors.Dict;
using Hors.Models;
using Hors.Utils;

namespace Hors.Recognizers
{
    public class MonthRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "([usxy])?M"; // [в] (прошлом|этом|следующем) марте
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            var year = userDate.Year;
            var yearFixed = false;

            // parse month
            var mStr = data.Tokens[match.Index + match.Groups[1].Length];
            var month = ParserUtils.FindIndex(mStr, Keywords.Months()) + 1;
            if (month == 0) month = userDate.Month;

            var monthPast = month < userDate.Month;
            var monthFuture = month > userDate.Month;
            
            // check if relative
            if (match.Groups[1].Success)
            {
                switch (match.Groups[1].Value)
                {
                    case "s": // previous
                        if (!monthPast) year--;
                        break;
                    case "u": // current
                        break;
                    case "y": // current-next
                        if (monthPast) year++;
                        break;
                    case "x": // next
                        if (!monthFuture) year++;
                        break;
                }

                yearFixed = true;
            }
            
            // create date
            var date = new AbstractPeriod
            {
                Date = new DateTime(year, month, 1)
            };
            
            // fix month and maybe year
            date.Fix(FixPeriod.Month);
            if (yearFixed) date.Fix(FixPeriod.Year);
            
            // remove and insert
            data.RemoveAndInsert(match.Index, match.Length, date);

            return true;
        }
    }
}