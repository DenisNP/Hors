using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hors.Models;
using Hors.Recognizers;
using Hors.Utils;

namespace Hors
{
    public class HorsTextParser
    {
        private readonly List<Recognizer> _recognizers = DefaultRecognizers();

        public HorsParseResult Parse(string text, DateTime userDate)
        {
            var tokens = Regex.Split(text, "[^а-яА-ЯёЁa-zA-Z0-9-]");
            return Parse(tokens, userDate, text);
        }

        public HorsParseResult Parse(IEnumerable<string> tokensList, DateTime userDate, string sourceText = null)
        {
            var tokens = tokensList.ToList();

            var data = new DatesRawData
            {
                Dates = new List<AbstractPeriod>(Enumerable.Repeat<AbstractPeriod>(null, tokens.Count)),
                Pattern = string.Join("", tokens.Select(ParserExtractors.CreatePatternFromToken)),
            };
            data.CreateTokens(tokens);
            
            // do work
            _recognizers.ForEach(r => r.ParseTokens(data, userDate));

            // collapse dates first batch
            Recognizer.ForAllMatches(data.GetPattern, "@(@|[fo]@)+", m => CollapseDates(m, data, userDate));
            Recognizer.ForAllMatches(data.GetPattern, "t@(@|t@)+", m => CollapseDates(m, data, userDate));
            Recognizer.ForAllMatches(data.GetPattern, "@{2,}", m => TakeFromAdjacent(m, data, userDate));

            // find periods
            var finalPeriods = new List<DateTimeToken>();
            Recognizer.ForAllMatches(
                data.GetPattern,
                "(([fo]?(@)t(@))|([fo]?(@)))",
                m => CreateDatePeriod(m, data, userDate, finalPeriods)
                );
            
            // if any dates overlap in source string, stretch them
            FixOverlap(finalPeriods);
            
            // return result
            var srcText = sourceText ?? string.Join(" ", tokens);
            return new HorsParseResult(srcText, data.Tokens.Select(t => t.Value).ToList(), finalPeriods);
        }

        private bool CreateDatePeriod(Match match, DatesRawData data, DateTime userDate, List<DateTimeToken> finalPeriods)
        {
            DateTimeToken dateToSave;
            
            // check matches
            if (match.Groups[3].Success && match.Groups[4].Success)
            {
                // this is the period "from" - "to"
                var fromDate = data.Dates[match.Groups[3].Index];
                var toDate = data.Dates[match.Groups[4].Index];

                var fromDateCopy = fromDate.CopyOf();
                fromDateCopy.Fixed &= (byte)~toDate.Fixed;
                fromDateCopy.SpanDirection = 0;
                
                var toDateCopy = toDate.CopyOf();
                toDateCopy.Fixed &= (byte)~fromDate.Fixed;
                toDateCopy.SpanDirection = 0;
                
                // set empty periods from each other
                AbstractPeriod.CollapseTwo(fromDate, toDateCopy);
                AbstractPeriod.CollapseTwo(toDate, fromDateCopy);

                // final dates
                var fromToken = ConvertToToken(fromDate, userDate);
                var toToken = ConvertToToken(toDate, userDate);
                var dateTo = toToken.DateTo;
                // var resolution = toDate.MaxFixed();
                
                // correct period if end less than start
                /*while (dateTo < fromToken.DateFrom)
                {
                    switch (resolution)
                    {
                        case FixPeriod.Time:
                            dateTo = dateTo.AddDays(1);
                            break;
                        case FixPeriod.Day:
                            dateTo = dateTo.AddDays(7);
                            break;
                        case FixPeriod.Week:
                            dateTo = dateTo.AddMonths(1);
                            break;
                        case FixPeriod.Month:
                            dateTo = dateTo.AddYears(1);
                            break;
                    }
                }*/

                dateToSave = new DateTimeToken
                {
                    DateFrom = fromToken.DateFrom,
                    DateTo = dateTo,
                    Type = DateTimeTokenType.Period
                };
            }
            else
            {
                // this is single date
                var singleDate = data.Dates[match.Groups[6].Index];
                dateToSave = ConvertToToken(singleDate, userDate);
            }

            // set period start and end indexes in source string
            dateToSave.SetEdges(
                data.EdgesByIndex(match.Index).Start,
                data.EdgesByIndex(match.Index + match.Length - 1).End
                );
                
            // save it to data
            var nextIndex = finalPeriods.Count;
            finalPeriods.Add(dateToSave);
            
            // fix final pattern
            data.Pattern = $"{data.Pattern.Substring(0, match.Index)}" +
                           (match.Index + match.Length < data.Pattern.Length 
                               ? $"${data.Pattern.Substring(match.Index + match.Length)}" 
                               : ""
                           );
            
            data.Tokens[match.Index] = new TextToken(
                $"{{{nextIndex}}}",
                dateToSave.StartIndex,
                dateToSave.EndIndex
                );
            
            data.Dates[match.Index] = null;
            
            if (match.Length > 1)
            {
                data.Tokens.RemoveRange(match.Index + 1, match.Length - 1);
                data.Dates.RemoveRange(match.Index + 1, match.Length - 1);
            }
            
            // we always modify initial object so always return true
            return true;
        }

        private DateTimeToken ConvertToToken(AbstractPeriod datePeriod, DateTime userDate)
        {
            // fill gaps
            var minFixed = datePeriod.MinFixed();
            datePeriod.FixDownTo(minFixed);
            
            switch (minFixed)
            {
                case FixPeriod.Month:
                    datePeriod.Date = new DateTime(userDate.Year, datePeriod.Date.Month, datePeriod.Date.Day);
                    break;
                case FixPeriod.Day:
                    var newDate = AbstractPeriod.TakeDayOfWeekFrom(userDate, datePeriod.Date);
                    datePeriod.Date = new DateTime(newDate.Year, newDate.Month, newDate.Day);
                    break;
                case FixPeriod.TimeUncertain:
                case FixPeriod.Time:
                    datePeriod.Date = userDate;
                    break;
            }

            if (datePeriod.IsFixed(FixPeriod.Time) || datePeriod.IsFixed(FixPeriod.TimeUncertain))
            {
                datePeriod.Date = new DateTime(
                    datePeriod.Date.Year, 
                    datePeriod.Date.Month, 
                    datePeriod.Date.Day,
                    datePeriod.Time.Hours,
                    datePeriod.Time.Minutes,
                    0,
                    0
                );
            }
            else
            {
                datePeriod.Date = new DateTime(
                    datePeriod.Date.Year, 
                    datePeriod.Date.Month, 
                    datePeriod.Date.Day,
                    0,
                    0,
                    0,
                    0
                );
            }
            
            // determine period type and dates
            var token = new DateTimeToken
            {
                Type = DateTimeTokenType.Fixed,
                StartIndex = datePeriod.Start,
                EndIndex = datePeriod.End
            };

            // set period dates by resolution
            var maxFixed = datePeriod.MaxFixed();
            switch (maxFixed)
            {
                case FixPeriod.Year:
                    token.Type = DateTimeTokenType.Period;
                    token.DateFrom = new DateTime(datePeriod.Date.Year, 1, 1);
                    token.DateTo = new DateTime(
                        datePeriod.Date.Year,
                        12,
                        31,
                        23,
                        59,
                        59, 999
                    );
                    break;
                case FixPeriod.Month:
                    token.Type = DateTimeTokenType.Period;
                    token.DateFrom = new DateTime(datePeriod.Date.Year, datePeriod.Date.Month, 1);
                    token.DateTo = new DateTime(
                        datePeriod.Date.Year,
                        datePeriod.Date.Month,
                        DateTime.DaysInMonth(datePeriod.Date.Year, datePeriod.Date.Month),
                        23,
                        59,
                        59,
                        999
                    );
                    break;
                case FixPeriod.Week:
                    var dayOfWeek = (int) datePeriod.Date.DayOfWeek;
                    if (dayOfWeek == 0) dayOfWeek = 7;
                    token.Type = DateTimeTokenType.Period;
                    token.DateFrom = datePeriod.Date.AddDays(1 - dayOfWeek);
                    token.DateTo = datePeriod.Date.AddDays(7 - dayOfWeek)
                                   + new TimeSpan(0, 23, 59, 59, 999);
                    break;
                case FixPeriod.Day:
                    token.Type = DateTimeTokenType.Fixed;
                    token.DateFrom = datePeriod.Date;
                    token.DateTo = datePeriod.Date
                                   + new TimeSpan(0, 23, 59, 59, 999);
                    break;
                case FixPeriod.TimeUncertain:
                case FixPeriod.Time:
                    token.Type = DateTimeTokenType.Fixed;
                    token.DateFrom = datePeriod.Date;
                    token.DateTo = datePeriod.Date;
                    break;
            }

            if (datePeriod.SpanDirection != 0)
            {
                token.Type = datePeriod.SpanDirection == 1
                    ? DateTimeTokenType.SpanForward
                    : DateTimeTokenType.SpanBackward;
                token.Span = datePeriod.Span;
            }

            return token;
        }

        private static bool CollapseDates(Match match, DatesRawData data, DateTime userDate)
        {
            var seenTokens = 0;
            var skippedTokens = 0;
            while (seenTokens < match.Length - 1)
            {
                var currentIndex = match.Index + skippedTokens;
                var currentDate = data.Dates[currentIndex];
                var nextDate = data.Dates[currentIndex + 1];

                if (nextDate == null || AbstractPeriod.CollapseTwo(currentDate, nextDate))
                {
                    data.Dates.RemoveAt(currentIndex + 1);
                    data.Tokens.RemoveAt(currentIndex + 1);
                    data.Pattern = data.Pattern.Remove(currentIndex + 1, 1);
                    
                    // add next token end index to current date
                    if (nextDate == null)
                    {
                        var nextToken = data.Tokens[currentIndex + 1];
                        currentDate.End = nextToken.End;
                    }
                }
                else
                {
                    skippedTokens++;
                }

                seenTokens++;
            }
            return skippedTokens == 0;
        }
        
        private bool TakeFromAdjacent(Match match, DatesRawData data, DateTime userDate)
        {
            for (var i = match.Index; i < match.Index + match.Length; i++)
            {
                // for current date take unfixed information from left and from right adjacent
                var prevDate = i > match.Index ? data.Dates[i - 1] : null;
                var currentDate = data.Dates[i];
                var nextDate = i < match.Index + match.Length - 1 ? data.Dates[i + 1] : null;

                // take from left only non fixed here
                if (prevDate != null)
                {
                    var prevDateCopy = prevDate.CopyOf();
                    prevDateCopy.Fixed &= (byte)~currentDate.Fixed;
                    AbstractPeriod.CollapseTwo(currentDate, prevDateCopy);
                }

                // take from right same way
                if (nextDate != null)
                {
                    var nextDateCopy = nextDate.CopyOf();
                    nextDateCopy.Fixed &= (byte)~currentDate.Fixed;
                    AbstractPeriod.CollapseTwo(currentDate, nextDateCopy);
                }
            }

            // this method doesn't modify tokens or array
            return false;
        }

        private static List<Recognizer> DefaultRecognizers()
        {
            return new List<Recognizer>
            {
                new HolidaysRecognizer(),
                new DaysMonthRecognizer(),
                new MonthRecognizer(),
                new RelativeDayRecognizer(),
                new TimeSpanRecognizer(),
                new YearRecognizer(),
                new RelativeDateRecognizer(),
                new DayOfWeekRecognizer(),
                new TimeRecognizer(),
                new PartOfDayRecognizer()
            };
        }

        public void SetRecognizers(params Recognizer[] recognizers)
        {
            _recognizers.Clear();
            AddRecognizers(recognizers);
        }

        public void AddRecognizers(params Recognizer[] recognizers)
        {
            _recognizers.AddRange(recognizers);
        }

        public void RemoveRecognizers(params Type[] typesToRemove)
        {
            _recognizers.RemoveAll(r => typesToRemove.Any(t => t == r.GetType()));
        }

        private void FixOverlap(List<DateTimeToken> finalPeriods)
        {
            var skippedDates = new HashSet<DateTimeToken>();
            foreach (var period in finalPeriods)
            {
                if (!skippedDates.Contains(period))
                {
                    var overlapPeriods = finalPeriods
                        .Where(p => p.OvelappingWith(period) && !skippedDates.Contains(p))
                        .ToList();
                    var minIndex = overlapPeriods.Select(p => p.StartIndex).Min();
                    var maxIndex = overlapPeriods.Select(p => p.EndIndex).Max();

                    foreach (var p in overlapPeriods)
                    {
                        p.StartIndex = minIndex;
                        p.EndIndex = maxIndex;
                        skippedDates.Add(p);
                    }
                }
            }
        }
    }
}