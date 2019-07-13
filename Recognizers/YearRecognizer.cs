using System;
using System.Text.RegularExpressions;
using Hors.Models;
using Hors.Utils;

namespace Hors.Recognizers
{
    public class YearRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "(1)Y?|(0)Y"; // [в] 15 году/2017 (году)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            // just year number
            int.TryParse(data.Tokens[match.Index].Value, out var n);
            var year = ParserUtils.GetYearFromNumber(n);

            // insert date
            var date = new AbstractPeriod
            {
                Date = new DateTime(year, 1, 1)
            };
            date.Fix(FixPeriod.Year);
            
            // remove and insert
            data.ReplaceTokensByDates(match.Index, match.Length, date);

            return true;
        }
    }
}