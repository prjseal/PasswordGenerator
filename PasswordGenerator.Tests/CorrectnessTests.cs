using System;
using System.Linq;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class CorrectnessTests
    {
        /// <summary>A deterministic <see cref="IRandomSource" /> that always returns 0.</summary>
        private class ZeroRandomSource : IRandomSource
        {
            public int NextInt(int maxExclusive) => 0;
        }

        [Test]
        public void CryptoRandomSource_NextInt_ReachesEveryIndexIncludingTop()
        {
            var rng = new CryptoRandomSource();
            var seen = new bool[10];
            for (var i = 0; i < 20000; i++)
                seen[rng.NextInt(10)] = true;

            Assert.IsTrue(seen.All(x => x), "Every index 0..9 (including the top index) must be reachable.");
        }

        [Test]
        public void CryptoRandomSource_NextInt_NeverReturnsOutOfRange()
        {
            var rng = new CryptoRandomSource();
            for (var i = 0; i < 20000; i++)
            {
                var value = rng.NextInt(7);
                Assert.GreaterOrEqual(value, 0);
                Assert.Less(value, 7);
            }
        }

        [Test]
        public void CryptoRandomSource_NextInt_IsApproximatelyUniform()
        {
            var rng = new CryptoRandomSource();
            const int range = 16;
            const int draws = 160000;
            var counts = new int[range];

            for (var i = 0; i < draws; i++)
                counts[rng.NextInt(range)]++;

            var expected = draws / (double)range;
            foreach (var count in counts)
                Assert.That(Math.Abs(count - expected) / expected, Is.LessThan(0.1),
                    "Selection should be approximately uniform (no modulo bias).");
        }

        [Test]
        public void CryptoRandomSource_NextInt_NonPositiveThrows()
        {
            var rng = new CryptoRandomSource();
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(-1));
        }

        [Test]
        public void Next_AllClassesIncluded_AlwaysContainsOneOfEachClass()
        {
            var pwd = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: true,
                includeSpecial: true, passwordLength: 4);

            for (var i = 0; i < 1000; i++)
            {
                var result = pwd.Next();
                Assert.AreEqual(4, result.Length);
                Assert.IsTrue(result.Any(char.IsLower), "missing lowercase");
                Assert.IsTrue(result.Any(char.IsUpper), "missing uppercase");
                Assert.IsTrue(result.Any(char.IsDigit), "missing digit");
                Assert.IsTrue(result.Any(c => !char.IsLetterOrDigit(c)), "missing special");
            }
        }

        [Test]
        public void Next_DeterministicRandomSource_ProducesDeterministicOutput()
        {
            var settings = new PasswordSettings(includeLowercase: false, includeUppercase: false,
                includeNumeric: true, includeSpecial: false, passwordLength: 4, maximumAttempts: 1,
                usingDefaults: false);
            var pwd = new Password(settings, new ZeroRandomSource());

            Assert.AreEqual("0000", pwd.Next());
        }

        [Test]
        public void Next_InvalidLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Password(3).Next());
            Assert.Throws<ArgumentException>(() => new Password(257).Next());
        }

        [Test]
        public void Next_EmptyCustomSpecialSet_ThrowsArgumentException()
        {
            var pwd = new Password().IncludeSpecial("   ");
            Assert.Throws<ArgumentException>(() => pwd.Next());
        }

        [Test]
        public void Next_NeverReturnsTryAgainOrErrorString()
        {
            var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeNumeric().IncludeSpecial("[]{}^_=");
            for (var i = 0; i < 200; i++)
            {
                var result = pwd.Next();
                Assert.AreNotEqual("Try again", result);
                Assert.IsFalse(result.StartsWith("Password length invalid"));
            }
        }

        [Test]
        public void TryNext_ValidSettings_ReturnsTrueAndPassword()
        {
            var pwd = new Password();
            Assert.IsTrue(pwd.TryNext(out var password));
            Assert.AreEqual(16, password.Length);
        }

        [Test]
        public void TryNext_InvalidSettings_ReturnsFalseAndNull()
        {
            var pwd = new Password(3);
            Assert.IsFalse(pwd.TryNext(out var password));
            Assert.IsNull(password);
        }
    }
}
