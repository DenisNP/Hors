using System;
using System.Text.RegularExpressions;
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
                var hours = 0;
                switch (match.Groups[2].Value)
                {
                    case "r": // morning 
                        hours = 9;
                        break;
                    case "a": // day
                    case "d":
                    case "n": // noon
                        hours = 12;
                        break;
                    case "v": // evening
                        hours = 17;
                        break;
                    case "g": // night
                        hours = 23;
                        break;
                }

                if (hours != 0)
                {
                    var date = new AbstractPeriod
                    {
                        Time = new TimeSpan(hours, 0, 0)
                    };
                    date.Fix(FixPeriod.TimeUncertain);
                
                    // remove and insert
                    var startIndex = match.Index;
                    var length = match.Length - 1; // skip date at the beginning or ar the end
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
                    data.ReplaceTokensByDates(startIndex, length, date);

                    return true;
                }
            }

            return false;
        }
    }
}