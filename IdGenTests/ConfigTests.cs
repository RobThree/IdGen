using IdGen;
using IdGen.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdGenTests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void IdGenerator_GetFromConfig_CreatesCorrectGenerator1()
        {
            var target = AppConfigFactory.GetFromConfig("foo");

            Assert.AreEqual(123, target.Id);
            Assert.AreEqual(new DateTime(2016, 1, 2, 12, 34, 56, DateTimeKind.Utc), target.Options.TimeSource.Epoch);
            Assert.AreEqual(39, target.Options.IdStructure.TimestampBits);
            Assert.AreEqual(11, target.Options.IdStructure.GeneratorIdBits);
            Assert.AreEqual(13, target.Options.IdStructure.SequenceBits);
            Assert.AreEqual(TimeSpan.FromMilliseconds(50), target.Options.TimeSource.TickDuration);
            Assert.AreEqual(SequenceOverflowStrategy.Throw, target.Options.SequenceOverflowStrategy);
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_CreatesCorrectGenerator2()
        {
            var target = AppConfigFactory.GetFromConfig("baz");

            Assert.AreEqual(2047, target.Id);
            Assert.AreEqual(new DateTime(2016, 2, 29, 0, 0, 0, DateTimeKind.Utc), target.Options.TimeSource.Epoch);
            Assert.AreEqual(21, target.Options.IdStructure.TimestampBits);
            Assert.AreEqual(21, target.Options.IdStructure.GeneratorIdBits);
            Assert.AreEqual(21, target.Options.IdStructure.SequenceBits);
            Assert.AreEqual(TimeSpan.FromTicks(7), target.Options.TimeSource.TickDuration);
            Assert.AreEqual(SequenceOverflowStrategy.SpinWait, target.Options.SequenceOverflowStrategy);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IdGenerator_GetFromConfig_IsCaseSensitive() => AppConfigFactory.GetFromConfig("Foo");

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IdGenerator_GetFromConfig_ThrowsOnNonExisting() => AppConfigFactory.GetFromConfig("xxx");


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IdGenerator_GetFromConfig_ThrowsOnInvalidIdStructure() => AppConfigFactory.GetFromConfig("e1");

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void IdGenerator_GetFromConfig_ThrowsOnInvalidEpoch() => AppConfigFactory.GetFromConfig("e2");

        [TestMethod]
        public void IdGenerator_GetFromConfig_ReturnsSameInstanceForSameName()
        {
            var target1 = AppConfigFactory.GetFromConfig("foo");
            var target2 = AppConfigFactory.GetFromConfig("foo");

            Assert.IsTrue(ReferenceEquals(target1, target2));
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_ParsesEpochCorrectly()
        {
            Assert.AreEqual(new DateTime(2016, 1, 2, 12, 34, 56, DateTimeKind.Utc), AppConfigFactory.GetFromConfig("foo").Options.TimeSource.Epoch);
            Assert.AreEqual(new DateTime(2016, 2, 1, 1, 23, 45, DateTimeKind.Utc), AppConfigFactory.GetFromConfig("bar").Options.TimeSource.Epoch);
            Assert.AreEqual(new DateTime(2016, 2, 29, 0, 0, 0, DateTimeKind.Utc), AppConfigFactory.GetFromConfig("baz").Options.TimeSource.Epoch);
        }

        [TestMethod]
        public void IdGenerator_GetFromConfig_ParsesTickDurationCorrectly()
        {
            Assert.AreEqual(TimeSpan.FromMilliseconds(50), AppConfigFactory.GetFromConfig("foo").Options.TimeSource.TickDuration);
            Assert.AreEqual(new TimeSpan(1, 2, 3), AppConfigFactory.GetFromConfig("bar").Options.TimeSource.TickDuration);
            Assert.AreEqual(TimeSpan.FromTicks(7), AppConfigFactory.GetFromConfig("baz").Options.TimeSource.TickDuration);

            // Make sure the default tickduration is 1 ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(1), AppConfigFactory.GetFromConfig("nt").Options.TimeSource.TickDuration);
        }

        [TestMethod]
        public void IdGeneratorElement_Property_Setters()
        {
            // We create an IdGeneratorElement from code and compare it to an IdGeneratorElement from config.
            var target = new IdGeneratorElement()
            {
                Name = "newfoo",
                Id = 123,
                Epoch = new DateTime(2016, 1, 2, 12, 34, 56, DateTimeKind.Utc),
                TimestampBits = 39,
                GeneratorIdBits = 11,
                SequenceBits = 13,
                TickDuration = TimeSpan.FromMilliseconds(50),
                SequenceOverflowStrategy = SequenceOverflowStrategy.Throw
            };
            var expected = AppConfigFactory.GetFromConfig("foo");

            Assert.AreEqual(expected.Id, target.Id);
            Assert.AreEqual(expected.Options.TimeSource.Epoch, target.Epoch);
            Assert.AreEqual(expected.Options.IdStructure.TimestampBits, target.TimestampBits);
            Assert.AreEqual(expected.Options.IdStructure.GeneratorIdBits, target.GeneratorIdBits);
            Assert.AreEqual(expected.Options.IdStructure.SequenceBits, target.SequenceBits);
            Assert.AreEqual(expected.Options.TimeSource.TickDuration, target.TickDuration);
            Assert.AreEqual(expected.Options.SequenceOverflowStrategy, target.SequenceOverflowStrategy);
        }
    }
}