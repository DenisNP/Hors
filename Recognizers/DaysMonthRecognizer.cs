using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hors.Dict;
using Hors.Models;
using Hors.Utils;

namespace Hors.Recognizers
{
    public class DaysMonthRecognizer : Recognizer
    {
        internal override string GetRegexPattern()
        {
            return "((0N?)+)(M|#)"; // 26 и 27 января/числа (2017 (года)/17 года)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            var dates = new List<AbstractPeriod>();
            var monthFixed = false;

            // parse month
            var mStr = data.Tokens[match.Index + match.Groups[1].Length];
            var month = ParserUtils.FindIndex(mStr, Keywords.Months()) + 1;
            if (month == 0) month = userDate.Month; // # instead M
            else monthFixed = true;
            
            // create dates
            for (var i = match.Index; i < match.Index + match.Groups[1].Length; i++)
            {
                var t = data.Tokens[i];
                var day = -1;
                int.TryParse(t, out day);
                if (day <= 0) continue;
                
                // current token is number, store it as a day
                var period = new AbstractPeriod
                {
                    Date = new DateTime(userDate.Year, month, ParserUtils.GetDayValidForMonth(userDate.Year, month, day))
                };   
                
                // fix from week to day, and year/month if it was
                period.Fix(FixPeriod.Week, FixPeriod.Day);
                if (monthFixed) period.Fix(FixPeriod.Month);
                
                // store
                dates.Add(period);
            }
            
            // remove all scanned tokens
            RemoveRange(data, match.Index, match.Length);
            
            // insert new dates
            if (dates.Count > 0)
            {
                InsertDates(data, match.Index, dates.ToArray());
            }

            return true;
        }
    }
}