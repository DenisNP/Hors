using System;
using System.Linq;
using Hors.Models;
using NUnit.Framework;

namespace Hors.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestJanuary()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "10 января событие",
                new DateTime(2019, 10, 13), 3
            );
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(10, date.DateFrom.Day);
            Assert.AreEqual(1, date.DateFrom.Month);
            Assert.AreEqual(2020, date.DateFrom.Year);
        }

        [Test]
        public void TestTimePeriodBeforeDay()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "с 5 до 7 вечера в понедельник будет событие",
                new DateTime(2019, 10, 13), 3
            );
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(17, date.DateFrom.Hour);
            Assert.AreEqual(19, date.DateTo.Hour);
            Assert.AreEqual(14, date.DateFrom.Day);
            Assert.AreEqual(14, date.DateTo.Day);
        }
        
        [Test]
        public void TestTimePeriodSimple()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "с 10 до 13 событие",
                new DateTime(2019, 10, 13), 3
            );
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(10, date.DateFrom.Hour);
            Assert.AreEqual(13, date.DateTo.Hour);
        }

        [Test]
        public void TestDaytime()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "Завтра в час обед и продлится он час с небольшим",
                new DateTime(2019, 10, 14), 3
            );
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(13, date.DateFrom.Hour);
        }
        
        [Test]
        public void TestNighttime()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "Завтра в 2 ночи полнолуние, а затем в 3 часа ночи новолуние и наконец в 12 часов ночи игра.",
                new DateTime(2020, 01, 01)
            );
            
            Assert.AreEqual(3, result.Dates.Count);
            
            var firstDate = result.Dates[0];
            Assert.AreEqual(DateTimeTokenType.Fixed, firstDate.Type);
            Assert.AreEqual(2, firstDate.DateFrom.Hour);

            var secondDate = result.Dates[1];
            Assert.AreEqual(DateTimeTokenType.Fixed, secondDate.Type);
            Assert.AreEqual(3, secondDate.DateFrom.Hour);

            var thirdDate = result.Dates[2];
            Assert.AreEqual(DateTimeTokenType.Fixed, thirdDate.Type);
            Assert.AreEqual(0, thirdDate.DateFrom.Hour);
            Assert.AreEqual(1, thirdDate.DateFrom.Day);
        }

        [Test]
        public void TestLongPeriod()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "С вечера следующей среды до четверти 10 утра понедельника в декабре можно будет наблюдать снег",
                new DateTime(2019, 10, 14), 3
            );
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(2019, date.DateFrom.Year);
            Assert.AreEqual(23, date.DateFrom.Day);
            Assert.AreEqual(10, date.DateFrom.Month);
            Assert.AreEqual(2, date.DateTo.Day);
            Assert.AreEqual(12, date.DateTo.Month);
            Assert.AreEqual(9, date.DateTo.Hour);
            Assert.AreEqual(15, date.DateTo.Minute);
        }

        [Test]
        public void TestCollapseComplex()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "В понедельник в 9 и 10 вечера",
                new DateTime(2019, 10, 13), 3
            );
            
            Assert.AreEqual(2, result.Dates.Count);
            
            var firstDate = result.Dates.First();
            Assert.AreEqual(2019, firstDate.DateFrom.Year);
            Assert.AreEqual(14, firstDate.DateFrom.Day);
            Assert.AreEqual(21, firstDate.DateFrom.Hour);
            
            var secondDate = result.Dates.Last();
            Assert.AreEqual(14, secondDate.DateFrom.Day);
            Assert.AreEqual(22, secondDate.DateFrom.Hour);
            
            // reverse order
            var resultR = parser.Parse(
                "В понедельник в 10 и 9 вечера",
                new DateTime(2019, 10, 13), 3
            );
            
            Assert.AreEqual(2, resultR.Dates.Count);
            
            var firstDateR = resultR.Dates.First();
            Assert.AreEqual(2019, firstDateR.DateFrom.Year);
            Assert.AreEqual(14, firstDateR.DateFrom.Day);
            Assert.AreEqual(22, firstDateR.DateFrom.Hour);
            
            var secondDateR = resultR.Dates.Last();
            Assert.AreEqual(14, secondDateR.DateFrom.Day);
            Assert.AreEqual(21, secondDateR.DateFrom.Hour);
        }

        [Test]
        public void TestMultipleSimple()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse(
                "Позавчера в 6:30 состоялось совещание, а завтра днём будет хорошая погода.",
                new DateTime(2019, 10, 13), 3
            );
            
            Assert.AreEqual(2, result.Dates.Count);
            
            var firstDate = result.Dates.First();
            Assert.AreEqual(2019, firstDate.DateFrom.Year);
            Assert.AreEqual(11, firstDate.DateFrom.Day);
            Assert.AreEqual(6, firstDate.DateFrom.Hour);
            Assert.AreEqual(30, firstDate.DateFrom.Minute);
            
            var secondDate = result.Dates.Last();
            Assert.AreEqual(2019, secondDate.DateFrom.Year);
            Assert.AreEqual(14, secondDate.DateFrom.Day);
            Assert.AreEqual(true, secondDate.HasTime);
        }

        [Test]
        public void TestCollapseDirection()
        {
            var strings = new[]
            {
                "В следующем месяце с понедельника буду ходить в спортзал!",
                "С понедельника в следующем месяце буду ходить в спортзал!"
            };
            
            var parser = new HorsTextParser();
            foreach (var s in strings)
            {
                var result = parser.Parse(s, new DateTime(2019, 10, 15), 3);
                Assert.AreEqual(1, result.Dates.Count);
                var date = result.Dates.First();
                Assert.AreEqual(2019, date.DateFrom.Year);
                Assert.AreEqual(4, date.DateFrom.Day);
                Assert.AreEqual(11, date.DateFrom.Month);
            }
        }

        [Test]
        public void TestWeekday()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("В следующем месяце во вторник состоится событие", new DateTime(2019, 10, 13), 3);
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(2019, date.DateFrom.Year);
            Assert.AreEqual(5, date.DateFrom.Day);
            Assert.AreEqual(11, date.DateFrom.Month);
            
            result = parser.Parse("Через месяц во вторник состоится событие", new DateTime(2019, 10, 13), 3);
            Assert.AreEqual(1, result.Dates.Count);
            date = result.Dates.First();
            Assert.AreEqual(2019, date.DateFrom.Year);
            Assert.AreEqual(12, date.DateFrom.Day);
            Assert.AreEqual(11, date.DateFrom.Month);
        }

        [Test]
        public void TestPunctuationAndIndexes()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("Через месяц, неделю и 2 дня состоится событие!", new DateTime(2019, 10, 13), 3);
            
            Assert.AreEqual(1, result.Dates.Count);
            Assert.AreEqual("состоится событие!", result.Text);
            
            result = parser.Parse("=== 26!%;марта   в 18:00 , , , будет *** экзамен!!", new DateTime(2019, 10, 13), 3);
            Assert.AreEqual(1, result.Dates.Count);
            Assert.AreEqual("=== , , , будет *** экзамен!!", result.Text);
        }

        [Test]
        public void TestCollapseDistanceDate()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("на следующей неделе будет событие в пятницу и будет оно в 12", new DateTime(2019, 10, 8), 3);
            
            Assert.AreEqual(1, result.Dates.Count);
            Assert.AreEqual("будет событие и будет оно", result.Text);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(18, date.DateFrom.Day);
            Assert.AreEqual(12, date.DateFrom.Hour);
        }
        
        [Test]
        public void TestCollapseDistanceTime()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("в четверг будет событие в 16 0 0", new DateTime(2019, 10, 8), 3);
            
            Assert.AreEqual(1, result.Dates.Count);
            Assert.AreEqual("будет событие", result.Text.Trim());
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(16, date.DateFrom.Hour);
            Assert.AreEqual(10, date.DateFrom.Day);
            
            result = parser.Parse("завтра встреча с другом в 12", new DateTime(2019, 10, 11), 5);
            Assert.AreEqual(1, result.Dates.Count);
            date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(12, date.DateFrom.Hour);
            Assert.AreEqual(12, date.DateFrom.Day);
            
            result = parser.Parse("в четверг будет хорошее событие в 16 0 0", new DateTime(2019, 10, 8), 2);
            
            Assert.AreEqual(2, result.Dates.Count);
            var dateFirst = result.Dates.First();
            var dateLast = result.Dates.Last();
            Assert.AreEqual(DateTimeTokenType.Fixed, dateFirst.Type);
            Assert.AreEqual(false, dateFirst.HasTime);
            Assert.AreEqual(true, dateLast.HasTime);
            Assert.AreEqual(16, dateLast.DateFrom.Hour);
            Assert.AreEqual(10, dateFirst.DateFrom.Day);
        }

        [Test]
        public void TestTimeAfterDay()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("в четверг 16 0 0 будет событие", new DateTime(2019, 10, 8));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(16, date.DateFrom.Hour);
            Assert.AreEqual(10, date.DateFrom.Day);
        }

        [Test]
        public void TestTimePeriod()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("В следующий четверг с 9 утра до 6 вечера важный экзамен!", new DateTime(2019, 9, 7));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(9, date.DateFrom.Hour);
            Assert.AreEqual(12, date.DateFrom.Day);
            Assert.AreEqual(9, date.DateFrom.Month);
            Assert.AreEqual(18, date.DateTo.Hour);
            Assert.AreEqual(12, date.DateTo.Day);
            Assert.AreEqual(9, date.DateTo.Month);
            Assert.AreEqual(2019, date.DateFrom.Year);
            Assert.AreEqual(2019, date.DateTo.Year);
        }
        
        [Test]
        public void TestComplexPeriod()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("хакатон с 12 часов 18 сентября до 12 часов 20 сентября", new DateTime(2019, 7, 7));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(12, date.DateFrom.Hour);
            Assert.AreEqual(18, date.DateFrom.Day);
            Assert.AreEqual(9, date.DateFrom.Month);
            Assert.AreEqual(12, date.DateTo.Hour);
            Assert.AreEqual(20, date.DateTo.Day);
            Assert.AreEqual(9, date.DateTo.Month);
            Assert.AreEqual(2019, date.DateFrom.Year);
            Assert.AreEqual(2019, date.DateTo.Year);
        }

        [Test]
        public void TestTimeBeforeDay()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("12 часов 12 сентября будет встреча", new DateTime(2019, 9, 7));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(12, date.DateFrom.Hour);
            Assert.AreEqual(12, date.DateFrom.Day);
            Assert.AreEqual(9, date.DateFrom.Month);
        }
        
        [Test]
        public void TestTimeHourOfDay()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("24 сентября в час дня", new DateTime(2019, 9, 7));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(true, date.HasTime);
            Assert.AreEqual(13, date.DateFrom.Hour);
        }

        [Test]
        public void TestFixPeriod()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("на выходных будет хорошо", new DateTime(2019, 9, 7));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(14, date.DateFrom.Day);
            Assert.AreEqual(15, date.DateTo.Day);
        }

        [Test]
        public void TestDatesPeriod()
        {
            var parser = new HorsTextParser();
            var strings = new[]
            {
                "с 11 по 15 сентября будет командировка",
                "11 по 15 сентября будет командировка",
                "с 11 до 15 сентября будет командировка"
            };
            
            foreach (var str in strings)
            {
                var result = parser.Parse(str, new DateTime(2019, 8, 6));
            
                Assert.AreEqual(1, result.Dates.Count);
                var date = result.Dates.First();
            
                Assert.AreEqual(DateTimeTokenType.Period, date.Type);
                Assert.AreEqual(11, date.DateFrom.Day);
                Assert.AreEqual(15, date.DateTo.Day);
                Assert.AreEqual(9, date.DateFrom.Month);
                Assert.AreEqual(9, date.DateTo.Month);
            }

            var resultSameMonth = parser.Parse(
                "с 11 до 15 числа будет командировка",
                new DateTime(2019, 9, 6)
            );
            Assert.AreEqual(1, resultSameMonth.Dates.Count);
            var dateSameMonth = resultSameMonth.Dates.First();
            
            Assert.AreEqual(DateTimeTokenType.Period, dateSameMonth.Type);
            Assert.AreEqual(11, dateSameMonth.DateFrom.Day);
            Assert.AreEqual(15, dateSameMonth.DateTo.Day);
            Assert.AreEqual(9, dateSameMonth.DateFrom.Month);
            Assert.AreEqual(9, dateSameMonth.DateTo.Month);
        }
        
        [Test]
        public void TestDaysOfWeek()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("во вторник встреча с заказчиком", new DateTime(2019, 9, 6));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();

            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(10, date.DateFrom.Day);
        }

        [Test]
        public void TestHolidays()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("в эти выходные еду на дачу", new DateTime(2019, 9, 2));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();

            Assert.AreEqual(DateTimeTokenType.Period, date.Type);
            Assert.AreEqual(7, date.DateFrom.Day);
            Assert.AreEqual(8, date.DateTo.Day);
        }
        
        [Test]
        public void TestHoliday()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("пойду гулять в следующий выходной", new DateTime(2019, 9, 2));
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();

            Assert.AreEqual(DateTimeTokenType.Fixed, date.Type);
            Assert.AreEqual(14, date.DateFrom.Day);
            Assert.AreEqual(14, date.DateTo.Day);
        }
    }
}