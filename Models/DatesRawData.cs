using System.Collections.Generic;
using System.Linq;

namespace Hors.Models
{
    
    public class DatesRawData
    {
        public List<TextToken> Tokens;
        public string Pattern;
        public List<AbstractPeriod> Dates;

        public string GetPattern()
        {
            return Pattern;
        }
        
        public void RemoveRange(int start, int count)
        {
            Tokens.RemoveRange(start, count);
            Dates.RemoveRange(start, count);
            Pattern = Pattern.Remove(start, count);
        }
        
        private void InsertDates(int index, params AbstractPeriod[] dates)
        {
            if (dates.Length == 0) return;
            InsertData(index, "@", "{}", dates);
        }

        public void ReplaceTokensByDates(int start, int removeCount, params AbstractPeriod[] dates)
        {
            var startIndex = Tokens[start].Start;
            var endIndex = Tokens[start + removeCount - 1].End;
            foreach (var date in dates)
            {
                if (date.End == 0)
                    date.SetEdges(startIndex, endIndex);
            }
            
            RemoveRange(start, removeCount);
            InsertDates(start, dates);
        }

        public void ReturnTokens(int index, string pattern, params TextToken[] tokens)
        {
            Dates.InsertRange(index, Enumerable.Repeat<AbstractPeriod>(null, tokens.Length));
            Tokens.InsertRange(index, tokens);
            
            var prefix = Pattern.Substring(0, index);
            var suffix = index < Pattern.Length ? Pattern.Substring(index) : "";
            
            Pattern = $"{prefix}{pattern}{suffix}";
        }

        public void InsertData(int index, string pattern, string token, params AbstractPeriod[] dates)
        {
            Dates.InsertRange(index, dates);
            Tokens.InsertRange(index, Enumerable.Repeat(new TextToken(token), dates.Length));

            var prefix = Pattern.Substring(0, index);
            var patterns = string.Join("", Enumerable.Repeat(pattern, dates.Length));
            var suffix = index < Pattern.Length ? Pattern.Substring(index) : "";
            
            Pattern = $"{prefix}{patterns}{suffix}";
        }

        private void FixZeros()
        {
            var i = 0;
            while (i < Tokens.Count - 1)
            {
                if (Tokens[i].Value == "0" && Tokens[i + 1].Value.Length == 1 && int.TryParse(Tokens[i + 1].Value, out _))
                {
                    // this in zero and number after it, delete zero
                    RemoveRange(i, 1);
                }
                else
                {
                    i++;
                }
            }
        }

        public void CreateTokens(List<string> tokens)
        {
            Tokens = new List<TextToken>();
            
            var len = 0;
            foreach (var currentToken in tokens)
            {
                var token = new TextToken(currentToken)
                {
                    Start = len,
                    End = len + currentToken.Length
                };
                
                Tokens.Add(token);
                len += currentToken.Length + 1; // +1 for separator symbol
            }
            
            FixZeros();
        }

        public IHasEdges EdgesByIndex(int tokenIndex)
        {
            var date = Dates[tokenIndex];
            if (date == null)
            {
                return Tokens[tokenIndex];
            }

            return date;
        }
    }
}