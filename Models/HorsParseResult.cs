using System.Collections.Generic;
using System.Linq;

namespace Hors.Models
{
    public class HorsParseResult
    {
        public string SourceText { get; }
        public List<string> Tokens { get; }
        public List<DateTimeToken> Dates { get; }
        public string Text { get; }

        private string _textWithTokens;

        public HorsParseResult(string sourceText, List<string> tokens, List<DateTimeToken> dates)
        {
            SourceText = sourceText;
            Tokens = tokens;
            Dates = dates;
            Text = CreateText(false);
        }

        private string CreateText(bool insertTokens)
        {
            var text = SourceText;
            var skippedDates = new HashSet<DateTimeToken>();
            
            // loop dates from last to first
            for (var i = Dates.Count - 1; i >= 0; i--)
            {
                var date = Dates[i];
                if (skippedDates.Contains(date)) continue;
                
                var sameDates = Dates.Where(d => d.StartIndex == date.StartIndex && !skippedDates.Contains(d)).ToList();
                var tokensToInsert = new List<string>();
                
                foreach (var oDate in sameDates)
                {
                    skippedDates.Add(oDate);
                    var indexInList = Dates.IndexOf(oDate);
                    tokensToInsert.Add($"{{{indexInList}}}");
                }

                text = text.Substring(0, date.StartIndex)
                       + (insertTokens ? string.Join(" ", tokensToInsert) : "")
                       + (date.EndIndex < text.Length ? text.Substring(date.EndIndex) : "");
            }

            return text;
        }

        public string CleanText => string.Join(" ", Tokens);

        public string TextWithTokens()
        {
            if (_textWithTokens == "")
            {
                _textWithTokens = CreateText(true);
            }

            return _textWithTokens;
        }

        public override string ToString()
        {
            return $"{CleanText} | {string.Join("; ", Dates.Select(d => d.ToString()))}";
        }
    }
}