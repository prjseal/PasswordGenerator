using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PasswordGeneratorTests
    {
        [TestMethod]
        public void PasswordGenerator_GivenNoSettings_ShouldReturn16Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator();
            string result = pwdGen.Next();
            Assert.AreEqual(16, result.Length);
        }

        [TestMethod]
        public void PasswordGenerator_GivenLength7_ShouldReturnLengthErrorMessage()
        {
            PasswordGenerator pwdGen = new PasswordGenerator(7);
            string result = pwdGen.Next();
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [TestMethod]
        public void PasswordGenerator_GivenLength129_ShouldReturnLengthErrorMessage()
        {
            PasswordGenerator pwdGen = new PasswordGenerator(129);
            string result = pwdGen.Next();
            Assert.AreEqual("Password length invalid. Must be between 8 and 128 characters long", result);
        }

        [TestMethod]
        public void PasswordGenerator_GivenLength128_ShouldReturn128Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator(128);
            string result = pwdGen.Next();
            Assert.AreEqual(128, result.Length);
        }

        [TestMethod]
        public void PasswordGenerator_IncludeLowercase_ShouldReturn16Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator().IncludeLowercase();
            string result = pwdGen.Next();
            Assert.AreEqual(16, result.Length);
        }

        [TestMethod]
        public void PasswordGenerator_LengthRequired50_ShouldReturn50Length()
        {
            PasswordGenerator pwdGen = new PasswordGenerator().LengthRequired(50);
            string result = pwdGen.Next();
            Assert.AreEqual(50, result.Length);
        }

    }
}
