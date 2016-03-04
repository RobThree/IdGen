using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdGenTests
{
    [TestClass]
    public class IdGenTests
    {
        private readonly DateTime TESTEPOCH = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
        public void GeneratorId_ShouldBePresent_InID1()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id 1023 so that all 10 bits
            // are set for the generator.
            var ts = new MockTimeSource(TESTEPOCH);
            var m = MaskConfig.Default;             // We use a default mask-config with 11 bits for the generator this time
            var g = new IdGenerator(1023, TESTEPOCH, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual((1 << m.GeneratorIdBits) - 1 << m.SequenceBits, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBePresent_InID2()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id 4095 so that all 12 bits
            // are set for the generator.
            var ts = new MockTimeSource(TESTEPOCH);
            var m = new MaskConfig(40, 12, 11);     // We use a custom mask-config with 12 bits for the generator this time
            var g = new IdGenerator(4095, TESTEPOCH, m, ts);

            // Make sure all expected bits are set
            Assert.AreEqual(-1 & ((1 << 12) - 1), g.Id);
            Assert.AreEqual((1 << 12) - 1 << 11, g.CreateId());
        }

        [TestMethod]
        public void GeneratorId_ShouldBeMasked_WhenReadFromProperty()
        {
            // We setup our generator so that the time (current - epoch) results in 0, generator id 1023 so that all 10
            // bits are set for the generator.
            var ts = new MockTimeSource(TESTEPOCH);
            var m = MaskConfig.Default;
            var g = new IdGenerator(1023, TESTEPOCH, m, ts);

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
            new IdGenerator(0, TESTEPOCH, new MaskConfig(41, 10, 11));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnGeneratorIdMoreThan31Bits()
        {
            new IdGenerator(0, TESTEPOCH, new MaskConfig(21, 32, 10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnSequenceMoreThan31Bits()
        {
            new IdGenerator(0, TESTEPOCH, new MaskConfig(21, 10, 32));
        }

        [TestMethod]
        [ExpectedException(typeof(SequenceOverflowException))]
        public void CreateId_Throws_OnSequenceOverflow()
        {
            var ts = new MockTimeSource(TESTEPOCH);
            var g = new IdGenerator(0, TESTEPOCH, new MaskConfig(41, 20, 2), ts);

            // We have a 2-bit sequence; generating 4 id's shouldn't be a problem
            for (int i = 0; i < 4; i++)
                Assert.AreEqual(i, g.CreateId());

            // However, if we invoke once more we should get an SequenceOverflowException
            g.CreateId();
        }

        //[TestMethod]
        //public void CheckAllCombinationsForMaskConfigs()
        //{
        //    var ts = new MockTimeSource(TESTEPOCH);

        //    for (byte i = 0; i < 32; i++)
        //    {
        //        var genid = (long)(1L << i) - 1;
        //        for (byte j = 2; j < 32; j++)
        //        {
        //            var g = new IdGenerator((int)genid, TESTEPOCH, new MaskConfig((byte)(63 - i - j), i, j), ts);
        //            var id = g.CreateId();
        //            Assert.AreEqual(genid << j, id);

        //            var id2 = g.CreateId();
        //            Assert.AreEqual((genid << j) + 1, id2);

        //            ts.NextTick();

        //            var id3 = g.CreateId();
        //            var id4 = g.CreateId();

        ////            System.Diagnostics.Trace.WriteLine(Convert.ToString(id, 2).PadLeft(64, '0'));
        ////            System.Diagnostics.Trace.WriteLine(Convert.ToString(id2, 2).PadLeft(64, '0'));
        ////            System.Diagnostics.Trace.WriteLine(Convert.ToString(id3, 2).PadLeft(64, '0'));
        ////            System.Diagnostics.Trace.WriteLine(Convert.ToString(id4, 2).PadLeft(64, '0'));

        //            ts.PreviousTick();
        //        }

        //    }
        //}

        [TestMethod]
        public void Constructor_UsesCorrect_Values()
        {
            Assert.AreEqual(123, new IdGenerator(123).Id);  //Make sure the test-value is not masked so it matches the expected value!
            Assert.AreEqual(TESTEPOCH, new IdGenerator(0, TESTEPOCH).Epoch);
        }

        [TestMethod]
        public void Enumerable_ShoudReturn_Ids()
        {
            var g = new IdGenerator(0);
            var ids = g.Take(1000).ToArray();

            Assert.AreEqual(1000, ids.Distinct().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSystemClockException))]
        public void CreateId_Throws_OnClockBackwards()
        {
            var ts = new MockTimeSource(DateTime.UtcNow);
            var m = MaskConfig.Default;
            var g = new IdGenerator(0, TESTEPOCH, m, ts);

            g.CreateId();
            ts.PreviousTick(); //Set clock back 1 'tick' (ms)
            g.CreateId();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnInvalidGeneratorId()
        {
            new IdGenerator(1024);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnEpochInFuture()
        {
            var ts = new MockTimeSource(TESTEPOCH);
            new IdGenerator(0, TESTEPOCH.AddTicks(1), MaskConfig.Default, ts);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSystemClockException))]
        public void Constructor_Throws_OnTimestampWraparound()
        {
            var mc = MaskConfig.Default;
            var ts = new MockTimeSource(mc.WraparoundDate(TESTEPOCH).AddMilliseconds(-1));  //Set clock to 1 ms before epoch
            var g = new IdGenerator(0, TESTEPOCH, MaskConfig.Default, ts);

            Assert.IsTrue(g.CreateId() > 0);   //Should succeed;
            ts.NextTick();
            g.CreateId();   //Should fail
        }

        [TestMethod]
        public void CreateMachineSpecificGenerator_Returns_IdGenerator()
        {
            var g = IdGenerator.CreateMachineSpecificGenerator();
            Assert.IsNotNull(g);
        }

        [TestMethod]
        public void CreateThreadSpecificGenerator_Returns_IdGenerator()
        {
            if (Environment.ProcessorCount > 1)
            {
                const int gcount = 100;  //Create a fair amount of generators
                var tasks = new Task<IdGenerator>[gcount];
                for (int i = 0; i < gcount; i++)
                    tasks[i] = Task.Run(() => IdGenerator.CreateThreadSpecificGenerator());
                Task.WaitAll(tasks);

                // Get all unique generator ID's in an array
                var generatorids = tasks.Select(t => t.Result.Id).Distinct().ToArray();
                // Make sure there's at least more than 1 different Id
                Assert.IsTrue(generatorids.Length > 1);
            }
        }

        [TestMethod]
        public void MaskConfigProperty_Returns_CorrectValue()
        {
            var md = MaskConfig.Default;
            var mc = new MaskConfig(21, 21, 21);

            Assert.ReferenceEquals(md, new IdGenerator(0, TESTEPOCH, md).MaskConfig);
            Assert.ReferenceEquals(mc, new IdGenerator(0, TESTEPOCH, mc).MaskConfig);
        }
    }
}