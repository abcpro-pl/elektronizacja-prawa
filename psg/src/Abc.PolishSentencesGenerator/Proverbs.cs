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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;

namespace Abc.PolishSentences {
    /// <summary>
    /// Text generator based on Polish proverbs and biblical verses.
    /// </summary>
    public abstract class Proverbs {
        private static List<String> Sentences = null;
        public static string Get(int minSentences = 1, int maxSentences = 5, int numParagraphs = 1, bool useHtml = false, bool whitoutNewLine = false) {
            if (Init()) {
                if (minSentences < 1) { throw new Exception("too few sentences!"); }
                if (minSentences > maxSentences) { throw new Exception("maxSentences wrong number!"); }
                if (numParagraphs < 1) { throw new Exception("Wrong paragraphs number!"); }

                var rand = new Random();
                var numSentences = rand.Next(minSentences, maxSentences);

                StringBuilder result = new StringBuilder();

                var sentencesRand = new Random();
                for (int p = 0; p < numParagraphs; p++) {
                    if (useHtml) {
                        result.Append("<p>");
                    }
                    for (int i = 0; i < numSentences; i++) {
                        var sentenceIndex = sentencesRand.Next(Sentences.Count - 1);
                        result.Append(Sentences[sentenceIndex]);
                        result.Append(" ");
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
            return default;
        }

        private static bool Init() {
            if (Sentences == null) {
                Sentences = new List<string>();
                var rm = new ResourceManager(typeof(Properties.SentencesResources));
                var resourceSet = rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                if (resourceSet != null) {
                    foreach (DictionaryEntry entry in resourceSet) {
                        Sentences.Add(entry.Value.ToString());
                    }
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                return true;
            }
        }
    }
}
