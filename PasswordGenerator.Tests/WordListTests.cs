using System;
using System.Linq;
using NUnit.Framework;

namespace PasswordGenerator.Tests
{
    public class WordListTests
    {
        // The EFF Large Wordlist is exactly 7,776 words; the entropy claims in
        // PassphraseGenerator depend on this count, so guard it explicitly.
        [Test]
        public void WordList_HasExactlyEffLargeCount()
        {
            Assert.That(WordList.Words.Length, Is.EqualTo(7776));
        }

        [Test]
        public void WordList_HasNoDuplicates()
        {
            var distinct = WordList.Words.Distinct(StringComparer.Ordinal).Count();
            Assert.That(distinct, Is.EqualTo(WordList.Words.Length));
        }

        [Test]
        public void WordList_WordsAreLowercaseLettersOrHyphen()
        {
            // The EFF list contains four legitimately hyphenated entries
            // (drop-down, felt-tip, t-shirt, yo-yo); everything else is a-z.
            foreach (var word in WordList.Words)
                Assert.That(word.All(c => (c >= 'a' && c <= 'z') || c == '-'), Is.True, word);
        }

        [Test]
        public void WordList_WordLengthsAreWithinEffBounds()
        {
            foreach (var word in WordList.Words)
                Assert.That(word.Length, Is.InRange(3, 9), word);
        }

        [Test]
        public void WordList_PerWordEntropyIsAboutTwelveNineBits()
        {
            var bitsPerWord = Math.Log(WordList.Words.Length, 2);
            Assert.That(bitsPerWord, Is.EqualTo(12.925).Within(0.001));
        }
    }
}
