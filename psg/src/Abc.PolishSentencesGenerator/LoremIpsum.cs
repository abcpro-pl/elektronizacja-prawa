/*=====================================================================================

	ABC PolishSentences 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.Text;

namespace Abc.PolishSentences {
    /// <summary>
    /// Lorem Ipsum is simply dummy text of the printing and typesetting industry. 
    ///  Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. 
    ///  It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. 
    ///  It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.
    /// </summary>
    public abstract class LoremIpsum {
        private static readonly string[] Words = {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
            "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
            "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};

        public static string Get(int minWords = 5, int maxWords = 20, int minSentences = 1, int maxSentences = 10, int numParagraphs = 1, bool useHtml = false, bool whitoutNewLine = false) {
            var rand = new Random();

            if (minSentences < 1) { throw new Exception("too few sentences!"); }
            if (minSentences > maxSentences) { throw new Exception("maxSentences wrong number!"); }

            if (minWords < 2) { throw new Exception("too few words!"); }
            if (minWords > maxWords) { throw new Exception("maxWords wrong number!"); }

            if (numParagraphs < 1) { throw new Exception("Wrong paragraphs number!"); }

            int numSentences = rand.Next(minSentences, maxSentences);
            int numWords = rand.Next(minWords, maxWords);

            StringBuilder result = new StringBuilder();

            for (int p = 0; p < numParagraphs; p++) {
                if (useHtml) {
                    result.Append("<p>");
                }
                for (int s = 0; s < numSentences; s++) {
                    for (int w = 0; w < numWords; w++) {
                        if (w > 0) { result.Append(" "); }
                        result.Append(Words[rand.Next(Words.Length)]);
                    }
                    result.Append(". ");
                }
                if (useHtml) {
                    result.Append("</p>");
                }
                else {
                    result.AppendLine();
                }
            }

            if (whitoutNewLine) {
                return result.ToString().UpperCaseFirst().Remove("\r\n");
            }
            return result.ToString().UpperCaseFirst();
        }
    }

    static class Extensions {
        public static string Remove(this string text, params string[] s) {
            if (!String.IsNullOrEmpty(text)) {
                if (s.Length > 0) {
                    foreach (string e in s) {
                        if (!String.IsNullOrEmpty(e)) {
                            text = text.Replace(e, String.Empty);
                        }
                    }
                }
            }

            return text;
        }
        public static string UpperCaseFirst(this string s) {
            if (string.IsNullOrEmpty(s)) {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
