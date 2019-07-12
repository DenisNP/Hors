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

    public struct DateTimeToken
    {
        public DateTimeTokenType Type;
        public DateTime DateFrom;
        public DateTime DateTo;
        public TimeSpan Span;

        public override string ToString()
        {
            return $"[Type={Type}, DateFrom={DateFrom.ToString(CultureInfo.CurrentCulture)}, DateTo={DateTo.ToString(CultureInfo.CurrentCulture)}, Span={Span.ToString()}]";
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