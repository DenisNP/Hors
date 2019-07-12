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

        protected static void RemoveRange(DatesRawData data, int start, int count)
        {
            data.Tokens.RemoveRange(start, count);
            data.Dates.RemoveRange(start, count);
            data.Pattern = data.Pattern.Remove(start, count);
        }

        protected static void InsertDates(DatesRawData data, int index, params AbstractPeriod[] dates)
        {
            InsertData(data, index, "@", dates);
        }

        protected static void RemoveAndInsert(DatesRawData data, int start, int removeCount, params AbstractPeriod[] dates)
        {
            RemoveRange(data, start, removeCount);
            InsertDates(data, start, dates);
        }

        protected static void InsertSymbol(DatesRawData data, int index, string prefix)
        {
            InsertData(data, index, prefix, new AbstractPeriod());
            data.Dates[index] = null;
        }

        private static void InsertData(DatesRawData data, int index, string placeholder, params AbstractPeriod[] dates)
        {
            data.Dates.InsertRange(index, dates);

            var placeholders = Enumerable.Repeat(placeholder, dates.Length).ToList();
            data.Tokens.InsertRange(index, placeholders);
            data.Pattern = $"{data.Pattern.Substring(0, index)}{string.Join("", placeholders)}{data.Pattern.Substring(index)}";
        }

        protected abstract string GetRegexPattern();

        protected abstract bool ParseMatch(DatesRawData data, Match match, DateTime userDate);
    }
}