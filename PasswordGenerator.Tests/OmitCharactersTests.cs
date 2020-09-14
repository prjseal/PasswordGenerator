using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordGenerator.Tests
{
    public class OmitCharactersTests
    {
        [Test]
        public void TestOmit_IlO0B8()
        {
            var charactersToOmit = "IlO0B8";
            var pwd = new Password(true, true, true, true, charactersToOmit, 8, 100);
            var result = pwd.NextGroup(1000);

            foreach (var password in result)
            {
                var index = password.IndexOfAny(charactersToOmit.ToCharArray());
                Assert.AreEqual(-1, index);
            }
        }

        [Test]
        public void TestNotOmit_IlO0B8()
        {
            var pwd = new Password(true, true, true, true, null, 8, 100);
            var result = pwd.NextGroup(1000);

            var charactersToOmit = "IlO0B8";
            long hasOmitedCharacters = 0;
            foreach (var password in result)
            {
                var index = password.IndexOfAny(charactersToOmit.ToCharArray());
                if (index >= 0)
                    hasOmitedCharacters++;
            }

            Assert.IsTrue(hasOmitedCharacters > 0);
        }
    }
}
