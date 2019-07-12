using System;
using System.Globalization;

namespace Hors.Models
{
    public class AbstractPeriod
    {
        public DateTime Date;
        public TimeSpan Time;
        public byte Fixed;
        public int SpanDirection;

        private static int _maxPeriod = -1;

        public void Fix(params FixPeriod[] fixes)
        {
            foreach (var f in fixes) Fixed |= (byte) f;
        }

        public void FixDownTo(FixPeriod period)
        {
            for (var i = GetMaxPeriod(); i >= 0; i--)
            {
                var toFix = (FixPeriod) Math.Pow(2, i);
                if (toFix < period)
                    return;

                Fix(toFix);
            }
        }

        private static int GetMaxPeriod()
        {
            if (_maxPeriod == -1)
            {
                var maxVal = 0;
                foreach (int p in Enum.GetValues(typeof(FixPeriod)))
                {
                    if (p > maxVal)
                    {
                        maxVal = p;
                    }
                }

                _maxPeriod = (int) Math.Log(maxVal, 2);
            }

            return _maxPeriod;
        }

        public AbstractPeriod CopyOf()
        {
            return new AbstractPeriod
            {
                Date = Date,
                Time = Time,
                Fixed = Fixed,
                SpanDirection = SpanDirection
            };
        }

        public FixPeriod MinFixed()
        {
            for (var i = GetMaxPeriod(); i >= 0; i--)
            {
                var p = (FixPeriod) Math.Pow(2, i);
                if (IsFixed(p))
                    return p;
            }

            return FixPeriod.None;
        }

        public FixPeriod MaxFixed()
        {
            for (var i = 0; i <= GetMaxPeriod(); i++)
            {
                var p = (FixPeriod) Math.Pow(2, i);
                if (IsFixed(p))
                    return p;
            }

            return FixPeriod.None;
        }

        public bool IsFixed(FixPeriod period)
        {
            return (Fixed & (int) period) > 0;
        }

        public override string ToString()
        {
            return
                $"[Date={Date.ToString(CultureInfo.CurrentCulture)}, Time={Time.ToString()}, Fixed={Convert.ToString(Fixed, 2)}]";
        }

        public static bool CollapseTwo(AbstractPeriod basePeriod, AbstractPeriod coverPeriod)
        {
            if ((basePeriod.Fixed & coverPeriod.Fixed) != 0) return false;
            if (basePeriod.SpanDirection != coverPeriod.SpanDirection) return false;
            
            // take year if it is not here, but is in other date
            if (!basePeriod.IsFixed(FixPeriod.Year) && coverPeriod.IsFixed(FixPeriod.Year))
            {
                basePeriod.Date = new DateTime(coverPeriod.Date.Year, basePeriod.Date.Month, basePeriod.Date.Day);
                basePeriod.Fix(FixPeriod.Year);
            }

            // take month if it is not here, but is in other date
            if (!basePeriod.IsFixed(FixPeriod.Month) && coverPeriod.IsFixed(FixPeriod.Month))
            {
                basePeriod.Date = new DateTime(basePeriod.Date.Year, coverPeriod.Date.Month, basePeriod.Date.Day);
                basePeriod.Fix(FixPeriod.Month);
            }

            // week and day
            if (!basePeriod.IsFixed(FixPeriod.Week) && coverPeriod.IsFixed(FixPeriod.Week))
            {
                // the week is in another date, check where is a day
                if (basePeriod.IsFixed(FixPeriod.Day))
                {
                    // set day of week, take date
                    basePeriod.Date = TakeDayOfWeekFrom(coverPeriod.Date, basePeriod.Date);
                    basePeriod.FixDownTo(FixPeriod.Week);
                }
                else if (coverPeriod.IsFixed(FixPeriod.Day))
                {
                    // the day not fixed here, take entire date from p
                    basePeriod.Date = coverPeriod.Date;
                    basePeriod.FixDownTo(FixPeriod.Day);
                }
                else
                {
                    // only week here, take it by taking a day
                    basePeriod.Date = new DateTime(basePeriod.Date.Year, basePeriod.Date.Month, coverPeriod.Date.Day);
                    basePeriod.FixDownTo(FixPeriod.Week);
                }
            }
            else if (basePeriod.IsFixed(FixPeriod.Week) && coverPeriod.IsFixed(FixPeriod.Day))
            {
                // here is a week, but day of week in other date
                basePeriod.Date = TakeDayOfWeekFrom(basePeriod.Date, coverPeriod.Date);
                basePeriod.FixDownTo(FixPeriod.Day);
            }
            
            // day
            if (!basePeriod.IsFixed(FixPeriod.Day) && coverPeriod.IsFixed(FixPeriod.Day))
            {
                basePeriod.Date = new DateTime(basePeriod.Date.Year, basePeriod.Date.Month, coverPeriod.Date.Day);
                basePeriod.FixDownTo(FixPeriod.Day);
            }
            
            // time
            if (!basePeriod.IsFixed(FixPeriod.Time) && coverPeriod.IsFixed(FixPeriod.Time))
            {
                basePeriod.Fix(FixPeriod.Time);
                if (!basePeriod.IsFixed(FixPeriod.TimeUncertain))
                {
                    basePeriod.Time = coverPeriod.Time;
                }
                else
                {
                    if (basePeriod.Time.Hours <= 12 && coverPeriod.Time.Hours > 12)
                    {
                        basePeriod.Time += new TimeSpan(12, 0, 0);
                    }
                }
            }
            
            if (!basePeriod.IsFixed(FixPeriod.TimeUncertain) && coverPeriod.IsFixed(FixPeriod.TimeUncertain))
            {
                basePeriod.Time = coverPeriod.Time;
                basePeriod.Fix(FixPeriod.TimeUncertain);
            }

            return true;
        }
        
        public static DateTime TakeDayOfWeekFrom(DateTime currentDate, DateTime takeFrom)
        {
            var needDow = (int)takeFrom.DayOfWeek;
            if (needDow == 0) needDow = 7;
            var currentDow = (int)currentDate.DayOfWeek;
            if (currentDow == 0) currentDow = 7;

            return currentDate.AddDays(needDow - currentDow);
        }
    }
}