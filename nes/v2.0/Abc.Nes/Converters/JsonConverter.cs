/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.IO;

namespace Abc.Nes.Converters {
    public class JsonConverter : IDisposable {
        public string GetJson(Document doc) {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
            return json;
        }
        
        public string WriteJson(Document doc, string filePath) {
            var json = GetJson(doc);
            if (json.IsNotNullOrEmpty()) {
                File.WriteAllText(filePath, json);
            }
            return json;
        }

        public Document LoadJson(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            Document result = Newtonsoft.Json.JsonConvert.DeserializeObject<Document>(File.ReadAllText(filePath));
            return result;
        }

        public Document ParseJson(string json) {
            Document result = Newtonsoft.Json.JsonConvert.DeserializeObject<Document>(json);
            return result;
        }
        public void Dispose() { }
    }
}
