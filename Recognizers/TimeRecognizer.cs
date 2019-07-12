using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hors.Models;

namespace Hors.Recognizers
{
    public class TimeRecognizer : Recognizer
    {
        internal override string GetRegexPattern()
        {
            return "([rvgd])?([fot])?(Q|H)?(h|(0)(h)?)((0)e?)?([rvgd])?"; // (в/с/до) (половину/четверть) час/9 (часов) (30 (минут)) (утра/дня/вечера/ночи)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            // determine if it is time
            if (
                (
                    match.Groups[2].Success // во фразе есть "в/с/до"
                    || (
                        match.Groups[3].Success // во фразе есть "половина/четверть" + число
                        && match.Groups[5].Success
                        ) 
                    || match.Groups[6].Success // во фразе есть "часов"
                    || match.Groups[1].Success // во фразе есть "утра/дня/вечера/ночи"
                    || match.Groups[9].Success
                    )
                && (
                    match.Groups[5].Success // либо число
                    || (
                        match.Groups[6].Success //  либо без числа, но "час дня/ночи"
                        && (match.Groups[9].Success || match.Groups[1].Success)
                        && "dg".Any(t => t.ToString() == match.Groups[9].Value)
                        )
                    )
                )
            {
                // hours and minutes
                var hours = match.Groups[5].Success ? int.Parse(data.Tokens[match.Groups[5].Index]) : 1;
                if (hours >= 0 && hours <= 23)
                {
                    // try minutes
                    var minutes = 0;
                    if (match.Groups[8].Success)
                    {
                        var m = int.Parse(data.Tokens[match.Groups[8].Index]);
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
                    if (hours <= 12 && (match.Groups[9].Success || match.Groups[1].Success))
                    {
                        // part of day
                        var part = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[9].Value;
                        switch (part)
                        {
                            case "d": // day
                                if (hours <= 4) hours += 12;
                                break;
                            case "v": // evening
                                if (hours <= 11) hours += 12;
                                break;
                            case "g": // night
                                hours += 12;
                                break;
                        }

                        if (hours == 24) hours = 0;
                        date.Fix(FixPeriod.Time);
                    }
                    
                    date.Time = new TimeSpan(hours, minutes, 0);

                    // remove and insert
                    RemoveRange(data, match.Index, match.Length);
                    InsertDates(data, match.Index, date);

                    if (match.Groups[2].Success && match.Groups[2].Value == "t")
                    {
                        // return "to" to correct period parsing
                        InsertData(data, match.Index, "t", new AbstractPeriod());
                        data.Dates[match.Index] = null;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}