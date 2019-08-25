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