using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;


namespace PasswordGenerator.Tests
{
    public class ObsoleteTests
    {
        [Test]
        public void PasswordGenerator_GivenNoSettings_ShouldReturn16Length()
        {
            Generator pwdGen = new Generator();
            string result = pwdGen.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_GivenLength3_ShouldReturnLengthErrorMessage()
        {
            Generator pwdGen = new Generator(3);
            string result = pwdGen.Next();
            Assert.AreEqual("Password length invalid. Must be between 4 and 256 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength257_ShouldReturnLengthErrorMessage()
        {
            Generator pwdGen = new Generator(257);
            string result = pwdGen.Next();
            Assert.AreEqual("Password length invalid. Must be between 4 and 256 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength256_ShouldReturn256Length()
        {
            Generator pwdGen = new Generator(256);
            string result = pwdGen.Next();
            Assert.AreEqual(256, result.Length);
        }

        [Test]
        public void PasswordGenerator_IncludeLowercase_ShouldReturn16Length()
        {
            Generator pwdGen = new Generator().IncludeLowercase();
            string result = pwdGen.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_LengthRequired50_ShouldReturn50Length()
        {
            Generator pwdGen = new Generator().LengthRequired(50);
            string result = pwdGen.Next();
            Assert.AreEqual(50, result.Length);
        }

        [Test]
        public void PasswordGenerator_16DigitNumeric_ShouldReturn16DigitNumericOnlyPassword()
        {
            Generator pwdGen = new Generator().IncludeNumeric();
            var result = pwdGen.Next();
            var pattern = @"^\d{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m.Success);
        }

        [Test]
        public void PasswordGenerator_16DigitLowercase_ShouldReturn16DigitLowercaseOnlyPassword()
        {
            Generator pwdGen = new Generator().IncludeLowercase();
            var result = pwdGen.Next();
            var pattern = @"^[a-z]{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m.Success);
        }
    }
}