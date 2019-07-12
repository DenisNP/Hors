using System.Collections.Generic;
using System.Linq;

namespace Hors.Models
{
    public class HorsParseResult
    {
        private readonly List<string> _tokens;
        public List<string> Tokens => _tokens;
        
        public string Text => string.Join(" ", _tokens);
        
        private readonly List<DateTimeToken> _dates;
        public List<DateTimeToken> Dates => _dates;

        public HorsParseResult(List<string> tokens, List<DateTimeToken> dates)
        {
            _tokens = tokens;
            _dates = dates;
        }

        public override string ToString()
        {
            return $"{Text} | {string.Join("; ", Dates.Select(d => d.ToString()))}";
        }
    }
}