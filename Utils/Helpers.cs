using System.Collections.Generic;
using System.Linq;

namespace Hors.Utils
{
    internal static class Helpers
    {
        /// <summary>
        /// TrimPunctuation from start and end of string.
        /// </summary>
        internal static string TrimPunctuation(string value, bool leaveValidSymbols = true)
        {
            // Count start punctuation.
            var removeFromStart = 0;
            const string validStart = "#{[{`\"'";
            foreach (var c in value)
            {
                if (char.IsPunctuation(c) && (!leaveValidSymbols || !validStart.Contains(c.ToString())))
                {
                    removeFromStart++;
                }
                else
                {
                    break;
                }
            }

            // Count end punctuation.
            var removeFromEnd = 0;
            const string validEnd = "!.?…)]}%\"'`";
            foreach (var c in value.Reverse())
            {
                if (char.IsPunctuation(c) && (!leaveValidSymbols || !validEnd.Contains(c.ToString())))
                {
                    removeFromEnd++;
                }
                else
                {
                    break;
                }
            }
            // No characters were punctuation.
            if (removeFromStart == 0 && removeFromEnd == 0)
            {
                return value;
            }
            // All characters were punctuation.
            if (removeFromStart == value.Length && removeFromEnd == value.Length)
            {
                return "";
            }
            // Substring.
            return value.Substring(removeFromStart, value.Length - removeFromEnd - removeFromStart);
        }

        internal static void SwapTwo<T>(this List<T> list, int firstIndex, int secondIndex)
        {
            T tmp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = tmp;
        }
    }
}