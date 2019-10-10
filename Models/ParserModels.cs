using System;
using System.Globalization;

namespace Hors.Models
{
    public enum FixPeriod
    {
        None = 0,
        Time = 1,
        TimeUncertain = 2,
        Day = 4,
        Week = 8,
        Month = 16,
        Year = 32
    }

    public class DateTimeToken
    {
        public DateTimeTokenType Type { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public TimeSpan Span { get; set; }
        public bool HasTime { get; set; }
        
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        private int _duplicateGroup = -1;

        public override string ToString()
        {
            return $"[Type={Type}, " +
                   $"DateFrom={DateFrom.ToString(CultureInfo.CurrentCulture)}, " +
                   $"DateTo={DateTo.ToString(CultureInfo.CurrentCulture)}, " +
                   $"Span={Span.ToString()}, " +
                   $"HasTime={HasTime}, " +
                   $"StartIndex={StartIndex}, " +
                   $"EndIndex={EndIndex}]";
        }

        public void SetEdges(int start, int end)
        {
            StartIndex = start;
            EndIndex = end;
        }

        public bool OvelappingWith(DateTimeToken other)
        {
            return StartIndex >= other.StartIndex && StartIndex <= other.EndIndex
                   || EndIndex >= other.StartIndex && EndIndex <= other.EndIndex;
        }

        internal void SetDuplicateGroup(int d)
        {
            _duplicateGroup = d;
        }

        internal int GetDuplicateGroup()
        {
            return _duplicateGroup;
        }
    }

    public enum DateTimeTokenType
    {
        Fixed,
        Period,
        SpanForward,
        SpanBackward,
    }

    public enum PartTime
    {
        None,
        Quarter,
        Half
    }

    public enum RelativeMode
    {
        None,
        Next,
        Previous,
        Current,
        CurrentNext
    }

    public enum Period
    {
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year,
        None,
    }

    public enum DayTime
    {
        None,
        Morning,
        Noon,
        Day,
        Evening,
        Night
    }
}