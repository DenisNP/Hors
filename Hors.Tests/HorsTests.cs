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
        public void TestWeekday()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("В следующем месяце во вторник состоится событие", new DateTime(2019, 10, 13), 3);
            
            Assert.AreEqual(1, result.Dates.Count);
            var date = result.Dates.First();
            Assert.AreEqual(5, date.DateFrom.Day);
            Assert.AreEqual(11, date.DateFrom.Month);
            
            result = parser.Parse("Через месяц во вторник состоится событие", new DateTime(2019, 10, 13), 3);
            Assert.AreEqual(1, result.Dates.Count);
            date = result.Dates.First();
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
        public void TestComplexPeriod()
        {
            var parser = new HorsTextParser();
            var result = parser.Parse("хакатон с 12 часов 18 сентября до 12 часов 20 сентября", new DateTime(2019, 9, 7));
            
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
                "с 11 до 15 сентября будет командировка",
                "с 11 до 15 числа будет командировка"
            };
            
            foreach (var str in strings)
            {
                var result = parser.Parse(str, new DateTime(2019, 9, 6));
            
                Assert.AreEqual(1, result.Dates.Count);
                var date = result.Dates.First();
            
                Assert.AreEqual(DateTimeTokenType.Period, date.Type);
                Assert.AreEqual(11, date.DateFrom.Day);
                Assert.AreEqual(15, date.DateTo.Day);
                Assert.AreEqual(9, date.DateFrom.Month);
                Assert.AreEqual(9, date.DateTo.Month);
            }
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