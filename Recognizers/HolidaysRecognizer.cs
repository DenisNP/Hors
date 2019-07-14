using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hors.Dict;
using Hors.Models;

namespace Hors.Recognizers
{
    public class HolidaysRecognizer : Recognizer
    {
        protected override string GetRegexPattern()
        {
            return "W";
        }

        protected override bool ParseMatch(DatesRawData data, Match match, DateTime userDate)
        {
            var token = data.Tokens[match.Index];
            data.RemoveRange(match.Index, 1);
            
            if (Morph.HasLemma(token.Value, Keywords.Holiday[0], Morph.LemmaSearchOptions.OnlySingular))
            {
                // singular
                var saturday = new TextToken(Keywords.Saturday[0])
                {
                    Start = token.Start,
                    End = token.End
                };
                data.ReturnTokens(match.Index, "D", saturday);
            }
            else
            {
                // plural
                var holidays = new[] {Keywords.Saturday[0], Keywords.TimeTo[0], Keywords.Sunday[0]}
                    .Select(k => new TextToken(k, token.Start, token.End))
                    .ToArray();
                data.ReturnTokens(match.Index, "DtD", holidays);
            }

            return true;
        }
    }
}