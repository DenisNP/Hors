using System;
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
            var token = data.Tokens[match.Index].Value;
            data.RemoveRange(match.Index, 1);
            
            if (Morph.HasLemma(token, Keywords.Holiday[0], Morph.LemmaSearchOptions.OnlySingular))
            {
                // singular
                data.InsertNonDate(match.Index, "D", Keywords.Saturday[0]);
            }
            else
            {
                // plural
                data.InsertNonDate(match.Index, "DtD", Keywords.Saturday[0], Keywords.TimeTo[0], Keywords.Sunday[0]);
            }

            return true;
        }
    }
}