using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace IdGenTests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void IdGenerator_GetFromConfig_CreatesCorrectGenerator()
        {
            var target = IdGenerator.GetFromConfig("foo");

            Assert.AreEqual(123, target.Id);
            Assert.AreEqual(new DateTime(2016, 1, 2, 12, 34, 56, DateTimeKind.Utc), target.Epoch);
            Assert.AreEqual(39, target.MaskConfig.TimestampBits);
            Assert.AreEqual(11, target.MaskConfig.GeneratorIdBits);
            Assert.AreEqual(13, target.MaskConfig.SequenceBits);
            Assert.AreEqual(TimeSpan.FromMilliseconds(50), target.TimeSource.TickDuration);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IdGenerator_GetFromConfig_IsCaseSensitive()
        {
            var target = IdGenerator.GetFromConfig("Foo");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IdGenerator_GetFromConfig_ThrowsOnNonExisting()
        {
            var target = IdGenerator.GetFromConfig("xxx");
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IdGenerator_GetFromConfig_ThrowsOnInvalidMask()
        {
            var target = IdGenerator.GetFromConfig("e1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void IdGenerator_GetFromConfig_ThrowsOnInvalidEpoch()
        {
            var target = IdGenerator.GetFromConfig("e2");
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_ReturnsSameInstanceForSameName()
        {
            var target1 = IdGenerator.GetFromConfig("foo");
            var target2 = IdGenerator.GetFromConfig("foo");

            Assert.ReferenceEquals(target1, target2);
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_ParsesEpochCorrectly()
        {
            Assert.AreEqual(new DateTime(2016, 1, 2, 12, 34, 56, DateTimeKind.Utc), IdGenerator.GetFromConfig("foo").Epoch);
            Assert.AreEqual(new DateTime(2016, 2, 1, 1, 23, 45, DateTimeKind.Utc), IdGenerator.GetFromConfig("bar").Epoch);
            Assert.AreEqual(new DateTime(2016, 2, 29, 0, 0, 0, DateTimeKind.Utc), IdGenerator.GetFromConfig("baz").Epoch);

            Assert.AreEqual(TimeSpan.FromMilliseconds(50), IdGenerator.GetFromConfig("foo").TimeSource.TickDuration);
            Assert.AreEqual(new TimeSpan(1, 2, 3), IdGenerator.GetFromConfig("bar").TimeSource.TickDuration);
            Assert.AreEqual(TimeSpan.FromTicks(7), IdGenerator.GetFromConfig("baz").TimeSource.TickDuration);
        }
    }
}
