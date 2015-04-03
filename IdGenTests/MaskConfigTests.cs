using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdGenTests
{
    [TestClass]
    public class MaskConfigTests
    {
        [TestMethod]
        public void DefaultMaskConfig_Matches_Expectations()
        {
            var m = MaskConfig.Default;

            Assert.AreEqual(41, m.TimestampBits);
            Assert.AreEqual(10, m.GeneratorIdBits);
            Assert.AreEqual(12, m.SequenceBits);
            Assert.AreEqual(63, m.TotalBits);

            // We should be able to generate a total of 63 bits worth of Id's
            Assert.AreEqual(long.MaxValue, (m.MaxGenerators * m.MaxIntervals * m.MaxSequenceIds) - 1);
        }

        [TestMethod]
        public void MaskConfig_CalculatesWraparoundInterval_Correctly()
        {
            // 40 bits of Timestamp should give us about 34 years worth of Id's
            Assert.AreEqual(34, (int)(new MaskConfig(40, 11, 12).WraparoundInterval().TotalDays / 365.25));
            // 41 bits of Timestamp should give us about 69 years worth of Id's
            Assert.AreEqual(69, (int)(new MaskConfig(41, 11, 11).WraparoundInterval().TotalDays / 365.25));
            // 42 bits of Timestamp should give us about 139 years worth of Id's
            Assert.AreEqual(139, (int)(new MaskConfig(42, 11, 10).WraparoundInterval().TotalDays / 365.25));
        }
    }
}
