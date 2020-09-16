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
using System.IO;
using System.Linq;
using System.Text;
using WeCantSpell.Hunspell;

namespace Abc.PolishSentences {
    /// <summary>
    /// Text generator based on the Polish dictionary.
    /// </summary>
    public abstract class Generator {
        private static string[] Words = null;
        public static string Get(int minWords = 5, int maxWords = 20, int minSentences = 1, int maxSentences = 10, int numParagraphs = 1, bool useHtml = false, bool whitoutNewLine = false) {
            if (minSentences < 1) { throw new Exception("too few sentences!"); }
            if (minSentences > maxSentences) { throw new Exception("maxSentences wrong number!"); }

            if (minWords < 2) { throw new Exception("too few words!"); }
            if (minWords > maxWords) { throw new Exception("maxWords wrong number!"); }

            if (numParagraphs < 1) { throw new Exception("Wrong paragraphs number!"); }

            if (Init()) {
                var rand = new Random();
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

            return default;
        }

        private static bool Init() {
            if (Words == null) {
                Words = WordList.CreateFromStreams(new MemoryStream(Properties.Resources.DIC), new MemoryStream(Properties.Resources.AFF)).RootWords.ToArray(); ;
                return true;
            }
            else { return true; }
        }
    }
}
