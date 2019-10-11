using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hors.Models;

namespace Hors.Recognizers
{
    public abstract class Recognizer
    {
        public void ParseTokens(DatesRawData data, DateTime userDate)
        {
            ForAllMatches(data.GetPattern, pattern: GetRegexPattern(), action: m => ParseMatch(data, m, userDate));
        }

        public static void ForAllMatches(Func<string> input, string pattern, Predicate<Match> action, bool reversed = false)
        {
            var matches = Regex.Matches(input.Invoke(), pattern);
            if (matches.Count == 0)
            {
                return;
            }

            var match = reversed ? matches[matches.Count - 1] : matches[0];
            var indexesToSkip = new HashSet<int>();

            while (match != null && match.Success)
            {
                if (!action.Invoke(match))
                {
                    indexesToSkip.Add(match.Index);
                }

                match = null;
                matches = Regex.Matches(input.Invoke(), pattern);
                for (var i = 0; i < matches.Count; i++)
                {
                    var index = reversed ? matches.Count - i - 1 : i;
                    if (!indexesToSkip.Contains(matches[index].Index))
                    {
                        match = matches[index];
                        break;
                    }
                }
            }
        }

        protected abstract string GetRegexPattern();

        protected abstract bool ParseMatch(DatesRawData data, Match match, DateTime userDate);
    }
}