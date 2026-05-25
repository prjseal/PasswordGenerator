using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class Phase4Tests
    {
        [Test]
        public void WithCharacters_RestrictsPoolToGivenCharacters()
        {
            var pwd = new Password().WithCharacters("ABC").LengthRequired(50);
            var result = pwd.Next();
            Assert.That(result.All(c => "ABC".Contains(c)), Is.True, result);
        }

        [Test]
        public void WithAllAscii_OnlyProducesPrintableAscii()
        {
            var pwd = new Password().WithAllAscii().LengthRequired(100);
            var result = pwd.Next();
            Assert.That(result.All(c => c >= 33 && c <= 126), Is.True, result);
        }

        [Test]
        public void ExcludeAmbiguous_RemovesLookAlikeCharacters()
        {
            var pwd = new Password(true, true, true, false, 200).ExcludeAmbiguous();
            for (var i = 0; i < 20; i++)
            {
                var result = pwd.Next();
                Assert.That(result.Any(c => CharacterFilter.AmbiguousCharacters.Contains(c)), Is.False, result);
            }
        }

        [Test]
        public void RequireAtLeast_GuaranteesMinimumPerClass()
        {
            var pwd = new Password(true, true, true, true, 16);
            pwd.RequireAtLeast(CharacterClass.Numeric, 4);

            for (var i = 0; i < 20; i++)
            {
                var result = pwd.Next();
                Assert.That(result.Count(char.IsDigit), Is.GreaterThanOrEqualTo(4), result);
            }
        }

        [Test]
        public void RequireAtLeast_EnablesClassThatWasNotIncluded()
        {
            var pwd = new Password(true, false, false, false, 16);
            pwd.RequireAtLeast(CharacterClass.Numeric, 3);
            Assert.That(pwd.Next().Count(char.IsDigit), Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void RequireAtLeast_ExceedingLength_FailsValidation()
        {
            var pwd = new Password(true, true, true, true, 4);
            pwd.RequireAtLeast(CharacterClass.Lowercase, 3);
            pwd.RequireAtLeast(CharacterClass.Uppercase, 3);

            Assert.That(pwd.TryNext(out var password), Is.False);
            Assert.That(password, Is.Null);
            Assert.Throws<ArgumentException>(() => pwd.Next());
        }

        [Test]
        public void RequireAtLeast_OnCustomPool_Throws()
        {
            var pwd = new Password().WithAllAscii();
            Assert.Throws<InvalidOperationException>(() => pwd.RequireAtLeast(CharacterClass.Numeric, 1));
        }

        [Test]
        public void EstimateEntropyBits_MatchesLengthTimesLog2PoolSize()
        {
            var pwd = new Password(true, false, false, false, 16); // lowercase only -> pool of 26
            var expected = 16 * Math.Log(26, 2);
            Assert.That(pwd.EstimateEntropyBits(), Is.EqualTo(expected).Within(1e-9));
        }

        [Test]
        public void ParameterlessGenerate_UsesDefaultBatchCount()
        {
            var pwd = new Password(16) { DefaultBatchCount = 5 };
            Assert.That(pwd.Generate().Count, Is.EqualTo(5));
        }

        // ----- Presets -----

        [Test]
        public void ForOtp_ProducesNumericCodeOfRequestedLength()
        {
            var otp = Password.ForOtp(6).Next();
            Assert.That(otp.Length, Is.EqualTo(6));
            Assert.That(otp.All(char.IsDigit), Is.True, otp);
        }

        [Test]
        public void ForApiKey_ProducesUrlSafeSecret()
        {
            var key = Password.ForApiKey(40).Next();
            Assert.That(key.Length, Is.EqualTo(40));
            Assert.That(key.All(c => CharacterFilter.UrlSafeCharacters.Contains(c)), Is.True, key);
        }

        [Test]
        public void ForOwasp_UsesPrintableAsciiAtRequestedLength()
        {
            var result = Password.ForOwasp(20).Next();
            Assert.That(result.Length, Is.EqualTo(20));
            Assert.That(result.All(c => c >= 33 && c <= 126), Is.True, result);
        }

        [Test]
        public void ForEnvironmentName_IsLowercaseDigitsWithoutAmbiguous()
        {
            var name = Password.ForEnvironmentName(12).Next();
            Assert.That(name.Length, Is.EqualTo(12));
            Assert.That(name.All(c => (char.IsLower(c) || char.IsDigit(c))
                                      && !CharacterFilter.AmbiguousCharacters.Contains(c)), Is.True, name);
        }

        [Test]
        public void ForPassphrase_ProducesRequestedWordsPlusNumber()
        {
            // Use '.' as the separator: a handful of EFF words are themselves hyphenated
            // (e.g. "t-shirt"), so splitting on '-' would over-split the phrase.
            var generator = Password.ForPassphrase(4, '.', capitalize: false, includeNumber: true);
            var phrase = generator.Next();
            var parts = phrase.Split('.');

            Assert.That(parts.Length, Is.EqualTo(5)); // 4 words + trailing number
            Assert.That(int.TryParse(parts[4], out _), Is.True, phrase);
            Assert.That(parts.Take(4).All(p => p.Length > 0 && p.All(c => char.IsLetter(c) || c == '-')),
                Is.True, phrase);
        }

        // ----- DI / appSettings precedence -----

        [Test]
        public void Di_CodeConfigOverridesConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Length"] = "10" })
                .Build();

            var services = new ServiceCollection();
            services.AddPasswordGenerator(configuration, o => o.Length = 20);

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            Assert.That(generator.Next().Length, Is.EqualTo(20));
        }

        [Test]
        public void Di_BindsExcludeAmbiguousAndDefaultBatchCount()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Length"] = "60",
                    ["IncludeSpecial"] = "false",
                    ["ExcludeAmbiguous"] = "true",
                    ["DefaultBatchCount"] = "4"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddPasswordGenerator(configuration);

            using var provider = services.BuildServiceProvider();
            var generator = provider.GetRequiredService<IPasswordGenerator>();

            Assert.That(generator.Generate().Count, Is.EqualTo(4));
            Assert.That(generator.Next().Any(c => CharacterFilter.AmbiguousCharacters.Contains(c)), Is.False);
        }
    }
}
