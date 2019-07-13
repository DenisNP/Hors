using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hors.Models;

namespace Hors.Recognizers
{
    public class TimeSpanRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "(i)?((0?[Ymwdhe]N?)+)([bl])?"; // (через) год и месяц и 2 дня 4 часа 10 минут (спустя/назад)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            if (match.Groups[1].Success ^ match.Groups[4].Success)
            {
                // if "after" of "before", but not both and not neither
                var letters = match.Groups[2].Value.Select(s => s.ToString()).ToList();
                var lastNumber = 1;
                var tokenIndex = match.Groups[2].Index;
                var direction = 1; // moving to the future
                if (match.Groups[4].Success && match.Groups[4].Value == "b")
                {
                    direction = -1; // "before"
                }
                
                var date = new AbstractPeriod
                {
                    SpanDirection = direction,
                };
                
                // save current day to offser object
                var offset = new DateTimeOffset(userDate);
                
                letters.ForEach(l =>
                {
                    switch (l)
                    {
                        case "N": // "and", skip it
                            break;
                        case "0": // number, store it
                            int.TryParse(data.Tokens[tokenIndex].Value, out lastNumber);
                            break;
                        case "Y": // year(s)
                            offset = offset.AddYears(direction * lastNumber);
                            date.FixDownTo(FixPeriod.Month);
                            lastNumber = 1;
                            break;
                        case "m": // month(s)
                            offset = offset.AddMonths(direction * lastNumber);
                            date.FixDownTo(FixPeriod.Week);
                            lastNumber = 1;
                            break;
                        case "w": // week(s)
                            offset = offset.AddDays(7 * direction * lastNumber);
                            date.FixDownTo(FixPeriod.Day);
                            lastNumber = 1;
                            break;
                        case "d": // day(s)
                            offset = offset.AddDays(direction * lastNumber);
                            date.FixDownTo(FixPeriod.Time);
                            lastNumber = 1;
                            break;
                        case "h": // hour(s)
                            offset = offset.AddHours(direction * lastNumber);
                            date.FixDownTo(FixPeriod.Time);
                            lastNumber = 1;
                            break;
                        case "e": // minute(s)
                            offset = offset.AddMinutes(direction * lastNumber);
                            date.FixDownTo(FixPeriod.Time);
                            break;
                    }

                    tokenIndex++;
                });
                
                // set date
                date.Date = new DateTime(offset.DateTime.Year, offset.DateTime.Month, offset.DateTime.Day);
                date.Time = new TimeSpan(offset.DateTime.Hour, offset.DateTime.Minute, 0);
                date.Span = offset - userDate;
                
                // remove and insert
                data.ReplaceTokensByDates(match.Index, match.Length, date);

                return true;
            }

            return false;
        }
    }
}