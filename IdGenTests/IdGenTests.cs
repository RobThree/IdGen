using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Linq;

namespace IdGenTests
{
    [TestClass]
    public class IdGenTests
    {
        [TestMethod]
        public void Sequence_ShouldIncrease_EveryInvocation()
        {
            // We setup our generator so that the time is 0, generator id 0 and we're only left with the sequence
            // increasing each invocation of CreateId();
            var ts = new MockTimeSource(0);
            var m = MaskConfig.Default;
            var g = new IdGenerator(0, m, ts);

            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            Assert.AreEqual(2, g.CreateId());
        }

        [TestMethod]
        public void Sequence_ShouldReset_EveryNewTick()
        {
            // We setup our generator so that the time is 0, generator id 0 and we're only left with the sequence
            // increasing each invocation of CreateId();
            var ts = new MockTimeSource(0);
            var m = MaskConfig.Default;
            var g = new IdGenerator(0, m, ts);

            Assert.AreEqual(0, g.CreateId());
            Assert.AreEqual(1, g.CreateId());
            ts.NextTick();
            // Since the timestamp has increased, we should now have a much higher value (since the timestamp is
            // shifted left a number of bits (specifically GeneratorIdBits + SequenceBits)
            Assert.AreEqual((1 << (m.GeneratorIdBits + m.SequenceBits)) + 0, g.CreateId());
            Assert.AreEqual((1 << (m.GeneratorIdBits + m.SequenceBits)) + 1, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBePresent_InID1()
        {
            // We setup our generator so that the time is 0 and generator id equals 1023 so that all 10 bits are set
            // for the generator.
            var ts = new MockTimeSource();
            var m = MaskConfig.Default;     // We use a default mask-config with 11 bits for the generator this time
            var g = new IdGenerator(1023, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual((1 << m.GeneratorIdBits) - 1 << m.SequenceBits, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBePresent_InID2()
        {
            // We setup our generator so that the time is 0 and generator id equals 4095 so that all 12 bits are set
            // for the generator.
            var ts = new MockTimeSource();
            var m = new MaskConfig(40, 12, 11); // We use a custom mask-config with 12 bits for the generator this time
            var g = new IdGenerator(4095, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual(-1 & ((1 << 12) - 1), g.Id);
            Assert.AreEqual((1 << 12) - 1 << 11, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBeMasked_WhenReadFromProperty()
        {
            // We setup our generator so that the time is 0 and generator id equals 1023 so that all 10 bits are set
            // for the generator.
            var ts = new MockTimeSource();
            var m = MaskConfig.Default;
            var g = new IdGenerator(1023, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual((1 << m.GeneratorIdBits) - 1, g.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_OnNullMaskConfig()
        {
            new IdGenerator(0, (MaskConfig)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Throws_OnNullTimeSource()
        {
            new IdGenerator(0, (ITimeSource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_Throws_OnMaskConfigNotExactly63Bits()
        {
            new IdGenerator(0, new MaskConfig(41, 10, 11));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnGeneratorIdMoreThan31Bits()
        {
            new IdGenerator(0, new MaskConfig(21, 32, 10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnSequenceMoreThan31Bits()
        {
            new IdGenerator(0, new MaskConfig(21, 10, 32));
        }

        [TestMethod]
        [ExpectedException(typeof(SequenceOverflowException))]
        public void CreateId_Throws_OnSequenceOverflow()
        {
            var ts = new MockTimeSource();
            var g = new IdGenerator(0, new MaskConfig(41, 20, 2), ts);

            // We have a 2-bit sequence; generating 4 id's shouldn't be a problem
            for (int i = 0; i < 4; i++)
                Assert.AreEqual(i, g.CreateId());

            // However, if we invoke once more we should get an SequenceOverflowException
            g.CreateId();
        }

        [TestMethod]
        public void Constructor_UsesCorrect_Values()
        {
            Assert.AreEqual(123, new IdGenerator(123).Id);  // Make sure the test-value is not masked so it matches the expected value!
            Assert.AreEqual(IdGenerator.DefaultEpoch, new IdGenerator(0).Epoch);
        }

        [TestMethod]
        public void Enumerable_ShoudReturn_Ids()
        {
            var g = new IdGenerator(0);
            var ids = g.Take(1000).ToArray();

            Assert.AreEqual(1000, ids.Distinct().Count());
        }

        [TestMethod]
        public void Enumerable_ShoudReturn_Ids_InterfaceExplicit()
        {
            var g = (IEnumerable)new IdGenerator(0);
            var ids = g.OfType<long>().Take(1000).ToArray();
            Assert.AreEqual(1000, ids.Distinct().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSystemClockException))]
        public void CreateId_Throws_OnClockBackwards()
        {
            var ts = new MockTimeSource(100);
            var m = MaskConfig.Default;
            var g = new IdGenerator(0, m, ts);

            g.CreateId();
            ts.PreviousTick(); // Set clock back 1 'tick', this results in the time going from "100" to "99"
            g.CreateId();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnInvalidGeneratorId_Positive()
        {
            new IdGenerator(1024);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnInvalidGeneratorId_Negative()
        {
            new IdGenerator(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSystemClockException))]
        public void Constructor_Throws_OnTimestampWraparound()
        {
            var m = MaskConfig.Default;
            var ts = new MockTimeSource(long.MaxValue);  // Set clock to 1 'tick' before wraparound
            var g = new IdGenerator(0, m, ts);

            Assert.IsTrue(g.CreateId() > 0);    // Should succeed;
            ts.NextTick();
            g.CreateId();                       // Should fail
        }

        [TestMethod]
        public void MaskConfigProperty_Returns_CorrectValue()
        {
            var md = MaskConfig.Default;
            var mc = new MaskConfig(21, 21, 21);

            Assert.ReferenceEquals(md, new IdGenerator(0, md).MaskConfig);
            Assert.ReferenceEquals(mc, new IdGenerator(0, mc).MaskConfig);
        }

        [TestMethod]
        public void Constructor_Overloads()
        {
            var i = 99;
            var ts = new MockTimeSource();
            var m = MaskConfig.Default;

            var epoch = DateTimeOffset.UtcNow;

            // Check all constructor overload variations
            Assert.AreEqual(i, new IdGenerator(i).Id);
            Assert.AreEqual(IdGenerator.DefaultEpoch, new IdGenerator(i).Epoch);

            Assert.AreEqual(i, new IdGenerator(i, epoch).Id);
            Assert.AreEqual(epoch, new IdGenerator(i, epoch).Epoch);

            Assert.AreEqual(i, new IdGenerator(i, ts).Id);
            Assert.AreSame(ts, new IdGenerator(i, ts).TimeSource);
            Assert.AreEqual(DateTimeOffset.MinValue, new IdGenerator(i, ts).Epoch);

            Assert.AreEqual(i, new IdGenerator(i, m).Id);
            Assert.AreSame(m, new IdGenerator(i, m).MaskConfig);
            Assert.AreEqual(IdGenerator.DefaultEpoch, new IdGenerator(i, m).Epoch);

            Assert.AreEqual(i, new IdGenerator(i, epoch, m).Id);
            Assert.AreSame(m, new IdGenerator(i, epoch, m).MaskConfig);
            Assert.AreEqual(epoch, new IdGenerator(i, epoch, m).Epoch);

            Assert.AreEqual(i, new IdGenerator(i, m, ts).Id);
            Assert.AreSame(ts, new IdGenerator(i, m, ts).TimeSource);
            Assert.AreSame(m, new IdGenerator(i, m, ts).MaskConfig);
            Assert.AreEqual(DateTimeOffset.MinValue, new IdGenerator(i, m, ts).Epoch);
        }
    }
}