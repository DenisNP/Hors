using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hors.Models;

namespace Hors.Recognizers
{
    public class TimeRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "([rvgd])?([fot])?(Q|H)?(h|(0)(h)?)((0)e?)?([rvgd])?"; // (в/с/до) (половину/четверть) час/9 (часов) (30 (минут)) (утра/дня/вечера/ночи)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            // determine if it is time
            if (
                match.Groups[5].Success // во фразе есть число
                || match.Groups[6].Success // во фразе есть "часов"
                || match.Groups[4].Success // во фразе есть "час"
                || match.Groups[1].Success // во начале есть "утра/дня/вечера/ночи"
                || match.Groups[9].Success // то же самое в конце
            )
            {
                if (!match.Groups[5].Success)
                {
                    // no number in phrase
                    var partOfDay = match.Groups[9].Success 
                        ? match.Groups[9].Value 
                        : match.Groups[1].Success 
                            ? match.Groups[1].Value 
                            : "";
                    
                    // no part of day AND no "from" token in phrase, quit
                    if (partOfDay != "d" && partOfDay != "g" && !match.Groups[2].Success)
                    {
                        return false;
                    }
                }
                
                // hours and minutes
                var hours = match.Groups[5].Success ? int.Parse(data.Tokens[match.Groups[5].Index].Value) : 1;
                if (hours >= 0 && hours <= 23)
                {
                    // try minutes
                    var minutes = 0;
                    if (match.Groups[8].Success)
                    {
                        var m = int.Parse(data.Tokens[match.Groups[8].Index].Value);
                        if (m >= 0 && m <= 59) minutes = m;
                    }
                    else if (match.Groups[3].Success && hours > 0)
                    {
                        switch (match.Groups[3].Value)
                        {
                            case "Q": // quarter
                                hours--;
                                minutes = 15;
                                break;
                            case "H": // half
                                hours--;
                                minutes = 30;
                                break;
                        }
                    }

                    // create time
                    var date = new AbstractPeriod();
                    date.Fix(FixPeriod.TimeUncertain);
                    if (hours > 12) date.Fix(FixPeriod.Time);

                    // correct time
                    if (hours <= 12)
                    {
                        var part = "d"; // default
                        if (match.Groups[9].Success || match.Groups[1].Success)
                        {
                            // part of day
                            part = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[9].Value;
                            date.Fix(FixPeriod.Time);
                        }
                        else
                        {
                            date.Fix(FixPeriod.TimeUncertain);
                        }
                        
                        switch (part)
                        {
                            case "d": // day
                                if (hours <= 4) hours += 12;
                                break;
                            case "v": // evening
                                if (hours <= 11) hours += 12;
                                break;
                            case "g": // night
                                if (hours >= 10) hours += 12;
                                break;
                        }

                        if (hours == 24) hours = 0;
                    }

                    date.Time = new TimeSpan(hours, minutes, 0);

                    // remove and insert
                    var toTime = data.Tokens[match.Index];
                    data.ReplaceTokensByDates(match.Index, match.Length, date);

                    if (match.Groups[2].Success && match.Groups[2].Value == "t")
                    {
                        // return "to" to correct period parsing
                        data.ReturnTokens(match.Index, "t", toTime);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}