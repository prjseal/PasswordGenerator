using System;
using System.Linq;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class Phase2Tests
    {
        [Test]
        public void NextGroup_ReturnsRequestedCount()
        {
            var pwd = new Password().LengthRequired(20);
            var result = pwd.NextGroup(50).ToList();
            Assert.That(result.Count, Is.EqualTo(50));
        }

        [Test]
        public void NextGroup_WithLongPasswords_ProducesUniqueValues()
        {
            var pwd = new Password().LengthRequired(32);
            var result = pwd.NextGroup(100).ToList();
            // At length 32 collisions are astronomically unlikely; all should be distinct.
            Assert.That(result.Distinct().Count(), Is.EqualTo(result.Count));
        }

        [Test]
        public void CustomSpecialPool_OnlyContainsTheGivenCharacters()
        {
            const string allowed = "!@#";
            var pwd = new Password().IncludeSpecial(allowed).LengthRequired(40);
            var result = pwd.Next();
            Assert.That(result.All(c => allowed.Contains(c)), Is.True, result);
        }

        [TestCase(4)]
        [TestCase(256)]
        public void Next_AtLengthBoundaries_ProducesPasswordOfThatLength(int length)
        {
            var pwd = new Password(length);
            var result = pwd.Next();
            Assert.That(result.Length, Is.EqualTo(length));
        }

        [Test]
        public void Next_WithNoCharacterClasses_Throws()
        {
            var settings = new PasswordSettings(false, false, false, false, 16, 10000, false);
            var pwd = new Password(settings);
            Assert.Throws<ArgumentException>(() => pwd.Next());
        }

        [Test]
        public void Constructor_WithNullRandomSource_Throws()
        {
            var settings = new PasswordSettings(true, true, true, true, 16, 10000, false);
            Assert.Throws<ArgumentNullException>(() => new Password(settings, null!));
        }
    }
}
