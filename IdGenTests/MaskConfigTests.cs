using IdGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            var mc_ms = new MockTimeSource();

            // 40 bits of Timestamp should give us about 34 years worth of Id's
            Assert.AreEqual(34, (int)(new MaskConfig(40, 11, 12).WraparoundInterval(mc_ms).TotalDays / 365.25));
            // 41 bits of Timestamp should give us about 69 years worth of Id's
            Assert.AreEqual(69, (int)(new MaskConfig(41, 11, 11).WraparoundInterval(mc_ms).TotalDays / 365.25));
            // 42 bits of Timestamp should give us about 139 years worth of Id's
            Assert.AreEqual(139, (int)(new MaskConfig(42, 11, 10).WraparoundInterval(mc_ms).TotalDays / 365.25));

            var mc_s = new MockTimeSource(TimeSpan.FromSeconds(0.1));

            // 40 bits of Timestamp should give us about 3484 years worth of Id's
            Assert.AreEqual(3484, (int)(new MaskConfig(40, 11, 12).WraparoundInterval(mc_s).TotalDays / 365.25));
            // 41 bits of Timestamp should give us about 6968 years worth of Id's
            Assert.AreEqual(6968, (int)(new MaskConfig(41, 11, 11).WraparoundInterval(mc_s).TotalDays / 365.25));
            // 42 bits of Timestamp should give us about 13936 years worth of Id's
            Assert.AreEqual(13936, (int)(new MaskConfig(42, 11, 10).WraparoundInterval(mc_s).TotalDays / 365.25));

            var mc_d = new MockTimeSource(TimeSpan.FromDays(1));

            // 21 bits of Timestamp should give us about 5741 years worth of Id's
            Assert.AreEqual(5741, (int)(new MaskConfig(21, 11, 31).WraparoundInterval(mc_d).TotalDays / 365.25));
            // 22 bits of Timestamp should give us about 11483 years worth of Id's
            Assert.AreEqual(11483, (int)(new MaskConfig(22, 11, 30).WraparoundInterval(mc_d).TotalDays / 365.25));
            // 23 bits of Timestamp should give us about 22966 years worth of Id's
            Assert.AreEqual(22966, (int)(new MaskConfig(23, 11, 39).WraparoundInterval(mc_d).TotalDays / 365.25));
        }

        [TestMethod]
        public void MaskConfig_CalculatesWraparoundDate_Correctly()
        {
            var m = MaskConfig.Default;
            var mc = new MockTimeSource(TimeSpan.FromMilliseconds(1));
            var d = m.WraparoundDate(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), mc);
            Assert.AreEqual(new DateTime(643346200555520000, DateTimeKind.Utc), d);
        }
    }
}
