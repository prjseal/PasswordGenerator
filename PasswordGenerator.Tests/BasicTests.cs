using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class BasicTests
    {
        [Test]
        public void PasswordGenerator_GivenNoSettings_ShouldReturn16Length()
        {
            var pwd = new Password();
            var result = pwd.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_GivenLength7_ShouldReturnLengthErrorMessage()
        {
            var pwd = new Password(7);
            var result = pwd.Next();
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength129_ShouldReturnLengthErrorMessage()
        {
            var pwd = new Password(129);
            var result = pwd.Next();
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength128_ShouldReturn128Length()
        {
            var pwd = new Password(128);
            var result = pwd.Next();
            Assert.AreEqual(128, result.Length);
        }

        [Test]
        public void PasswordGenerator_IncludeLowercase_ShouldReturn16Length()
        {
            var pwd = new Password().IncludeLowercase();
            var result = pwd.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_LengthRequired50_ShouldReturn50Length()
        {
            var pwd = new Password().LengthRequired(50);
            var result = pwd.Next();
            Assert.AreEqual(50, result.Length);
        }
        
        [Test]
        public void PasswordGenerator_10Passwords_ShouldReturn10DifferentPasswords()
        {
            var pwd = new Password().LengthRequired(50);
            var result = pwd.NextGroup(10);
            Assert.AreEqual(10, result.Count());
        }

        [Test]
        public void PasswordGenerator_16DigitNumeric_ShouldReturn16DigitNumericOnlyPassword()
        {
            var pwd = new Password().IncludeNumeric();
            var result = pwd.Next();
            var pattern = @"^\d{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m.Success);
        }

        [Test]
        public void PasswordGenerator_16DigitLowercase_ShouldReturn16DigitLowercaseOnlyPassword()
        {
            var pwd = new Password().IncludeLowercase();
            var result = pwd.Next();
            var pattern = @"^[a-z]{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m.Success);
        }
    }
}