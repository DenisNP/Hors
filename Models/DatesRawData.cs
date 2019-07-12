using System.Collections.Generic;
using System.Linq;

namespace Hors.Models
{
    
    public class DatesRawData
    {
        public List<string> Tokens;
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
        
        public void InsertDates(int index, params AbstractPeriod[] dates)
        {
            InsertData(index, "@", "{}", dates);
        }

        public void RemoveAndInsert(int start, int removeCount, params AbstractPeriod[] dates)
        {
            RemoveRange(start, removeCount);
            InsertDates(start, dates);
        }

        public void InsertNonDate(int index, string pattern, params string[] tokens)
        {
            Dates.InsertRange(index, Enumerable.Repeat<AbstractPeriod>(null, tokens.Length));
            Tokens.InsertRange(index, tokens);
            
            var prefix = Pattern.Substring(0, index);
            var suffix = Pattern.Substring(index);
            
            Pattern = $"{prefix}{pattern}{suffix}";
        }

        public void InsertData(int index, string pattern, string token, params AbstractPeriod[] dates)
        {
            Dates.InsertRange(index, dates);
            Tokens.InsertRange(index, Enumerable.Repeat(token, dates.Length));

            var prefix = Pattern.Substring(0, index);
            var patterns = string.Join("", Enumerable.Repeat(pattern, dates.Length));
            var suffix = Pattern.Substring(index);
            
            Pattern = $"{prefix}{patterns}{suffix}";
        }
    }
}