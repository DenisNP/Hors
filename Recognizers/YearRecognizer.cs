using System;
using System.Text.RegularExpressions;
using Hors.Models;
using Hors.Utils;

namespace Hors.Recognizers
{
    public class YearRecognizer : Recognizer
    {
        internal override string GetRegexPattern()
        {
            return "(1)Y?|(0)Y"; // [в] 15 году/2017 (году)
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            // just year number
            int.TryParse(data.Tokens[match.Index], out var n);
            var year = ParserUtils.GetYearFromNumber(n);

            // remove tokens
            RemoveRange(data, match.Index, match.Length);
            
            // insert date
            var date = new AbstractPeriod
            {
                Date = new DateTime(year, 1, 1)
            };
            date.Fix(FixPeriod.Year);
            
            InsertDates(data, match.Index, date);

            return true;
        }
    }
}