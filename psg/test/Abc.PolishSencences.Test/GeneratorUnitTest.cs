using System;
using System.Runtime.InteropServices;
using Abc.PolishSentences;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Abc.PolishSencences.Test {
    [TestClass]
    public class GeneratorUnitTest {
        [TestMethod]
        public void GetProverbs() {
            var minSentences = 1;
            var maxSentences = 5;
            var numParagraphs = 1;
            var useHtml = false;
            var whitoutNewLine = false;
            var result = Proverbs.Get(minSentences, maxSentences, numParagraphs, useHtml, whitoutNewLine);
            Assert.IsTrue(!String.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetGeneratedSentences() {
            var minWords = 5;
            var maxWords = 20;
            var minSentences = 1;
            var maxSentences = 10;
            var numParagraphs = 1;
            var useHtml = false;
            var whitoutNewLine = false;
            var result = Generator.Get(minWords, maxWords, minSentences, maxSentences, numParagraphs, useHtml, whitoutNewLine);
            Assert.IsTrue(!String.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void GetLoremIpsum() {
            var minWords = 5;
            var maxWords = 20;
            var minSentences = 1;
            var maxSentences = 10;
            var numParagraphs = 1;
            var useHtml = false;
            var whitoutNewLine = false;
            var result = LoremIpsum.Get(minWords, maxWords, minSentences, maxSentences, numParagraphs, useHtml, whitoutNewLine);
            Assert.IsTrue(!String.IsNullOrEmpty(result));
        }
    }
}
