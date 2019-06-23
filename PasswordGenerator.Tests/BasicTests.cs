using NUnit.Framework;
using PasswordGenerator.Services;

namespace PasswordGenerator.Tests
{
    public class BasicTests
    {
        [Test]
        public void PasswordGenerator_GivenNoSettings_ShouldReturn16Length()
        {
            var pwd = new PasswordService();
            var result = pwd.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_GivenLength7_ShouldReturnLengthErrorMessage()
        {
            var pwd = new PasswordService(7);
            var result = pwd.Next();
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength129_ShouldReturnLengthErrorMessage()
        {
            var pwd = new PasswordService(129);
            var result = pwd.Next();
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [Test]
        public void PasswordGenerator_GivenLength128_ShouldReturn128Length()
        {
            var pwd = new PasswordService(128);
            var result = pwd.Next();
            Assert.AreEqual(128, result.Length);
        }

        [Test]
        public void PasswordGenerator_IncludeLowercase_ShouldReturn16Length()
        {
            var pwd = new PasswordService().IncludeLowercase();
            var result = pwd.Next();
            Assert.AreEqual(16, result.Length);
        }

        [Test]
        public void PasswordGenerator_LengthRequired50_ShouldReturn50Length()
        {
            var pwd = new PasswordService().LengthRequired(50);
            var result = pwd.Next();
            Assert.AreEqual(50, result.Length);
        }
    }
}