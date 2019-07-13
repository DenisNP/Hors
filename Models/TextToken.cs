namespace Hors.Models
{
    public class TextToken : IHasEdges
    {
        public string Value;
        public int Start { get; set; }
        public int End { get; set; }
        
        public TextToken(string value)
        {
            Value = value;
        }

        public TextToken(string value, int startIndex, int endIndex)
        {
            Value = value;
            Start = startIndex;
            End = endIndex;
        }
    }
}