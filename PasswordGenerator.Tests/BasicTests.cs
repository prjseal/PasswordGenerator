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
        public void PasswordGenerator_GivenLength3_ShouldReturnLengthErrorMessage()
        {
            var pwd = new Password(3);
            var result = pwd.Next();
            Assert.AreEqual("Password length invalid. Must be between 4 and 256 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength257_ShouldReturnLengthErrorMessage()
        {
            var pwd = new Password(257);
            var result = pwd.Next();
            Assert.AreEqual("Password length invalid. Must be between 4 and 256 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength256_ShouldReturn128Length()
        {
            var pwd = new Password(256);
            var result = pwd.Next();
            Assert.AreEqual(256, result.Length);
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

        [Test]
        public void PasswordGenerator_SpecificSpecialCharacters_ShouldReturnPasswordWithSpecialCharacterFromSpecificSet()
        {
            var pwd = new Password().IncludeSpecial("*(&)_^");
            var result = pwd.Next();
            var pattern = @"^[*|(|&|)|_|^]{16}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m.Success);
        }

        [Test]
        public void PasswordGenerator_OneTimePasscode_ShouldReturn4DigitNumber()
        {
            var pwd = new Password(4).IncludeNumeric();
            var result = pwd.Next();
            var pattern = @"^\d{4}$";
            var m = Regex.Match(result, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m.Success);
        }

        [Test]
        public void PasswordGenerator_SpecificSpecialCharacters_ShouldNotReturnTryAgain()
        {
            var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeNumeric().IncludeSpecial("[]{}^_=");
            var result = pwd.Next();
            Assert.AreNotEqual("Try again", result);
        }

        [Test]
        public void PasswordGenerator_LengthOnly_ShouldNotThrowAnError()
        {
            var pwd = new Password(passwordLength: 21);
            string result = pwd.Next();
            Assert.AreEqual(21,result.Length);
        }

        [Test]
        public void PasswordGenerator_NoLengthTest_ShouldNotThrowAnError()
        {
            var pwd = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: true, includeSpecial: false);
            string result = pwd.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_ParametersWithLength_ShouldNotThrowAnError()
        {
            var pwd = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: true, includeSpecial: false, passwordLength: 21);
            string result = pwd.Next();
            Assert.AreEqual(21, result.Length);
        }

        [Test]
        public void PasswordGenerator_ParametersWithLengthAndMaxAttempts_ShouldNotThrowAnError()
        {
            var pwd = new Password(includeLowercase: true, includeUppercase: true, includeNumeric: true, includeSpecial: false, passwordLength: 24, maximumAttempts: 100);
            string result = pwd.Next();
            Assert.AreEqual(24, result.Length);
        }
    }
}