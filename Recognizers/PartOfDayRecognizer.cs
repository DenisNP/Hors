using System;
using System.Text.RegularExpressions;
using Hors.Dict;
using Hors.Models;

namespace Hors.Recognizers
{
    public class PartOfDayRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "(@)?f?([ravgdn])f?(@)?"; // (дата) (в/с) утром/днём/вечером/ночью (в/с) (дата)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            if (match.Groups[1].Success || match.Groups[3].Success)
            {
                var hourStart = 0;
                var hourEnd = 0;
                switch (match.Groups[2].Value)
                {
                    case "r": // morning 
                        hourStart = 5;
                        hourEnd = 11;
                        break;
                    case "a": // day
                    case "d":
                        hourStart = 11;
                        hourEnd = 15;
                        break;
                    case "n": // noon
                        hourStart = 12;
                        hourEnd = 12;
                        break;
                    case "v": // evening
                        hourStart = 15;
                        hourEnd = 23;
                        break;
                    case "g": // night
                        hourStart = 23;
                        hourEnd = 5;
                        break;
                }

                if (hourStart != 0)
                {
                    var date = new AbstractPeriod
                    {
                        Time = new TimeSpan(hourStart, 0, 0)
                    };
                    date.Fix(FixPeriod.TimeUncertain);

                    // remove and insert
                    var startIndex = match.Index;
                    var length = match.Length - 1; // skip date at the beginning or at the end
                    if (match.Groups[1].Success)
                    {
                        // skip first date
                        startIndex++;
                        if (match.Groups[3].Success)
                        {
                            // skip both dates at the beginning and at the end
                            length--;
                        }
                    }

                    if (hourEnd == hourStart)
                    {
                        data.ReplaceTokensByDates(startIndex, length, date);
                    }
                    else
                    {
                        var dateEnd = new AbstractPeriod
                        {
                            Time = new TimeSpan(hourEnd, 0, 0)
                        };
                        dateEnd.Fix(FixPeriod.TimeUncertain);
                        data.ReplaceTokensByDates(startIndex, length, date, dateEnd);
                        data.ReturnTokens(startIndex + 1, "t", new TextToken(Keywords.TimeTo[0]));
                    }

                    return true;
                }
            }

            return false;
        }
    }
}