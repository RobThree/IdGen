#if NET451
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
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_IsCaseSensitive()
        {
            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                var target = IdGenerator.GetFromConfig("Foo");
            });
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_ThrowsOnNonExisting()
        {
            Assert.ThrowsException<KeyNotFoundException>(() =>
            {
                var target = IdGenerator.GetFromConfig("xxx");
            });
        }


        [TestMethod]
        public void IdGenerator_GetFromConfig_ThrowsOnInvalidMask()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var target = IdGenerator.GetFromConfig("e1");
            });
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void IdGenerator_GetFromConfig_ThrowsOnInvalidEpoch()
        {
            Assert.ThrowsException<FormatException>(() =>
            {
                var target = IdGenerator.GetFromConfig("e2");
            });
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
        }
    }
}
#endif