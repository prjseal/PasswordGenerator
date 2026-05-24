using System;
using System.Text.RegularExpressions;
using NUnit.Framework;


namespace PasswordGenerator.Tests
{
    public class ObsoleteTests
    {
        [Test]
        public void PasswordGenerator_GivenNoSettings_ShouldReturn16Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator();
            string result = pwdGen.Next();
            Assert.That(result.Length, Is.EqualTo(16));
        }

        [Test]
        public void PasswordGenerator_GivenLength3_ShouldThrowArgumentException()
        {
            PasswordGenerator pwdGen = new PasswordGenerator(3);
            Assert.Throws<ArgumentException>(() => pwdGen.Next());
        }

        [Test]
        public void PasswordGenerator_GivenLength257_ShouldThrowArgumentException()
        {
            PasswordGenerator pwdGen = new PasswordGenerator(257);
            Assert.Throws<ArgumentException>(() => pwdGen.Next());
        }

        [Test]
        public void PasswordGenerator_GivenLength256_ShouldReturn256Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator(256);
            string result = pwdGen.Next();
            Assert.That(result.Length, Is.EqualTo(256));
        }

        [Test]
        public void PasswordGenerator_IncludeLowercase_ShouldReturn16Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator().IncludeLowercase();
            string result = pwdGen.Next();
            Assert.That(result.Length, Is.EqualTo(16));
        }

        [Test]
        public void PasswordGenerator_LengthRequired50_ShouldReturn50Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator().LengthRequired(50);
            string result = pwdGen.Next();
            Assert.That(result.Length, Is.EqualTo(50));
        }

        [Test]
        public void PasswordGenerator_16DigitNumeric_ShouldReturn16DigitNumericOnlyPassword()
        {
            PasswordGenerator pwdGen = new PasswordGenerator().IncludeNumeric();
            var result = pwdGen.Next();
            var pattern = @"^\d{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.That(m.Success, Is.True);
        }

        [Test]
        public void PasswordGenerator_16DigitLowercase_ShouldReturn16DigitLowercaseOnlyPassword()
        {
            PasswordGenerator pwdGen = new PasswordGenerator().IncludeLowercase();
            var result = pwdGen.Next();
            var pattern = @"^[a-z]{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.That(m.Success, Is.True);
        }
    }
}
