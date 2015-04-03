using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IdGenTests
{
    [TestClass]
    public class IdGenTests
    {
        private readonly DateTime TESTEPOCH = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void Sequence_ShouldIncrease_EveryInvocation()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id 0 and we're only
            // left with the sequence increasing each invocation of CreateId();
            var ts = new MockTimeSource(TESTEPOCH);
            var m = MaskConfig.Default;
            var g = new IdGenerator(0, TESTEPOCH, MaskConfig.Default, ts);

            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            Assert.AreEqual(2, g.CreateId());
        }

        [TestMethod]
        public void Sequence_ShouldReset_EveryNewTick()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id 0 and we're only
            // left with the sequence increasing each invocation of CreateId();
            var ts = new MockTimeSource(TESTEPOCH);
            var m = MaskConfig.Default;
            var g = new IdGenerator(0, TESTEPOCH, m, ts);

            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            ts.NextTick();
            // Since the timestamp has increased, we should now have a much higher value (since the timestamp is shifted
            // left a number of bits (specifically GeneratorIdBits + SequenceBits)
            Assert.AreEqual((1 << (m.GeneratorIdBits + m.SequenceBits)) + 0, g.CreateId());
            Assert.AreEqual((1 << (m.GeneratorIdBits + m.SequenceBits)) + 1, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBePresent_InID()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id -1 so that all 32 bits
            // are set for the generator.
            var ts = new MockTimeSource(TESTEPOCH);
            var m = MaskConfig.Default;
            var g = new IdGenerator(-1, TESTEPOCH, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual((1 << m.GeneratorIdBits) - 1 << m.SequenceBits, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBeMasked_WhenReadFromProperty()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id -1 so that all 32 bits
            // are set for the generator.
            var ts = new MockTimeSource(TESTEPOCH);
            var m = MaskConfig.Default;
            var g = new IdGenerator(-1, TESTEPOCH, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual((1 << m.GeneratorIdBits) - 1, g.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_OnNullMaskConfig()
        {
            new IdGenerator(0, TESTEPOCH, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_OnNullTimeSource()
        {
            new IdGenerator(0, TESTEPOCH, MaskConfig.Default, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_Throws_OnMaskConfigNotExactly63Bits()
        {
            new IdGenerator(0, TESTEPOCH, new MaskConfig { TimestampBits = 41, GeneratorIdBits = 10, SequenceBits = 11 });
        }
    }
}
