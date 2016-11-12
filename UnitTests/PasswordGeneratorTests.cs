using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PasswordGeneratorTests
    {
        [TestMethod]
        public void PasswordGenerator_Given8Length8_ShouldReturn8Length()
        {
            PasswordGeneratorSettings settings = new PasswordGeneratorSettings(true, false, false, false, 8);
            string result = PasswordGenerator.GetValidPassword(settings);
            Assert.AreEqual(8, result.Length);
        }

        [TestMethod]
        public void PasswordGenerator_GivenLength7_ShouldReturnLengthErrorMessage()
        {
            PasswordGeneratorSettings settings = new PasswordGeneratorSettings(true, false, false, false, 7);
            string result = PasswordGenerator.GetValidPassword(settings);
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [TestMethod]
        public void PasswordGenerator_GivenLength129_ShouldReturnLengthErrorMessage()
        {
            PasswordGeneratorSettings settings = new PasswordGeneratorSettings(true, false, false, false, 129);
            string result = PasswordGenerator.GetValidPassword(settings);
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [TestMethod]
        public void PasswordGenerator_GivenLength128_ShouldReturn128Length()
        {
            PasswordGeneratorSettings settings = new PasswordGeneratorSettings(true, false, false, false, 128);
            string result = PasswordGenerator.GetValidPassword(settings);
            Assert.AreEqual(128, result.Length);
        }
    }
}
