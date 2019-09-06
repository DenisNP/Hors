using System;
using System.Text.RegularExpressions;
using Hors.Dict;
using Hors.Models;
using Hors.Utils;

namespace Hors.Recognizers
{
    public class DatesPeriodRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "f?(0)[ot]0(M|#)"; // с 26 до 27 января/числа
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            var monthFixed = false;

            // parse month
            var mStr = data.Tokens[match.Index + match.Groups[4].Length].Value;
            var month = ParserUtils.FindIndex(mStr, Keywords.Months()) + 1;
            if (month == 0) month = userDate.Month; // # instead M
            else monthFixed = true;
            
            // set for first date same month as for second, ignore second (will parse further)
            var t = data.Tokens[match.Groups[1].Index];
            int.TryParse(t.Value, out var day);
            
            // current token is number, store it as a day
            var period = new AbstractPeriod
            {
                Date = new DateTime(
                    userDate.Year,
                    month,
                    ParserUtils.GetDayValidForMonth(userDate.Year, month, day)
                )
            };   
            
            // fix from week to day, and year/month if it was
            period.Fix(FixPeriod.Week, FixPeriod.Day);
            if (monthFixed) period.Fix(FixPeriod.Month);

            // replace
            data.ReplaceTokensByDates(match.Groups[1].Index, 1, period);

            return true;
        }
    }
}