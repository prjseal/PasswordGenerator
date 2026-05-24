using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class Phase1CorrectnessTests
    {
        // A deterministic random source that cycles a fixed sequence of values, so generation is
        // fully reproducible in tests (no crypto RNG involved).
        private class FixedRandomSource : IRandomSource
        {
            private readonly int[] _values;
            private int _index;

            public FixedRandomSource(params int[] values)
            {
                _values = values;
            }

            public int NextInt(int maxExclusive)
            {
                var value = _values[_index % _values.Length] % maxExclusive;
                _index++;
                return value;
            }
        }

        [Test]
        public void Next_WithInvalidLength_ThrowsArgumentException()
        {
            var pwd = new Password(3);
            Assert.Throws<ArgumentException>(() => pwd.Next());
        }

        [Test]
        public void TryNext_WithInvalidLength_ReturnsFalseAndDoesNotThrow()
        {
            var pwd = new Password(3);
            var ok = pwd.TryNext(out var result);
            Assert.That(ok, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryNext_WithValidSettings_ReturnsTrueAndPassword()
        {
            var pwd = new Password(16);
            var ok = pwd.TryNext(out var result);
            Assert.That(ok, Is.True);
            Assert.That(result!.Length, Is.EqualTo(16));
        }

        [Test]
        public void IncludeSpecial_WithEmptySet_ThrowsInsteadOfReturningTryAgain()
        {
            var pwd = new Password();
            pwd.IncludeLowercase().IncludeSpecial("   ");
            Assert.Throws<ArgumentException>(() => pwd.Next());
        }

        [Test]
        public void Next_WithAllClasses_AlwaysContainsOneOfEachClass()
        {
            // Deterministic guarantee: every generated password must contain each required class.
            for (var i = 0; i < 200; i++)
            {
                var pwd = new Password(includeLowercase: true, includeUppercase: true,
                    includeNumeric: true, includeSpecial: true, passwordLength: 8);
                var result = pwd.Next();

                Assert.That(result.Any(char.IsLower), Is.True, $"missing lowercase: {result}");
                Assert.That(result.Any(char.IsUpper), Is.True, $"missing uppercase: {result}");
                Assert.That(result.Any(char.IsDigit), Is.True, $"missing digit: {result}");
                Assert.That(result.Any(c => !char.IsLetterOrDigit(c)), Is.True, $"missing special: {result}");
            }
        }

        [Test]
        public void CryptoRandomSource_NextInt_IsInRangeAndReachesTopValue()
        {
            var rng = new CryptoRandomSource();
            var seen = new HashSet<int>();
            for (var i = 0; i < 20000; i++)
            {
                var v = rng.NextInt(10);
                Assert.That(v, Is.GreaterThanOrEqualTo(0));
                Assert.That(v, Is.LessThan(10));
                seen.Add(v);
            }

            // The old modulo implementation could never produce the top index; verify it now can.
            Assert.That(seen, Does.Contain(9), "top value (9) was never produced");
            Assert.That(seen.Count, Is.EqualTo(10), "not every value in range was produced");
        }

        [Test]
        public void CryptoRandomSource_NextInt_WithNonPositiveRange_Throws()
        {
            var rng = new CryptoRandomSource();
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(0));
        }

        [Test]
        public void Next_WithInjectedRandomSource_IsDeterministic()
        {
            var settingsA = new PasswordSettings(true, true, true, false, 12, 10000, false);
            var settingsB = new PasswordSettings(true, true, true, false, 12, 10000, false);

            var a = new Password(settingsA, new FixedRandomSource(0, 1, 2, 3, 4, 5));
            var b = new Password(settingsB, new FixedRandomSource(0, 1, 2, 3, 4, 5));

            Assert.That(a.Next(), Is.EqualTo(b.Next()));
        }
    }
}
