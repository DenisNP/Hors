using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void ForAllMatches(Func<string> input, string pattern, Predicate<Match> action)
        {
            var match = Regex.Match(input.Invoke(), pattern);
            var indexesToSkip = new HashSet<int>();

            while (match != null && match.Success)
            {
                if (!action.Invoke(match))
                {
                    indexesToSkip.Add(match.Index);
                }

                match = null;
                var matches = Regex.Matches(input.Invoke(), pattern);
                for (var i = 0; i < matches.Count; i++)
                {
                    if (!indexesToSkip.Contains(matches[i].Index))
                    {
                        match = matches[i];
                        break;
                    }
                }
            }
        }

        protected abstract string GetRegexPattern();

        protected abstract bool ParseMatch(DatesRawData data, Match match, DateTime userDate);
    }
}