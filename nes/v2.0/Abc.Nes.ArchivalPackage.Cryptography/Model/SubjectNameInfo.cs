using System.Collections.Generic;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public class SubjectNameInfo : Dictionary<string, string> {
        public SubjectNameInfo(string subjectName) {
            string[] sc = subjectName.Split(',');
            if (sc.Length > 0) {
                foreach (string t in sc) {
                    string[] sci = t.Split('=');
                    if (sci.Length > 1) {
                        var key = sci[0].Trim();
                        var value = sci[1].Trim();
                        if (this.ContainsKey(key)) {
                            this[key] += $" {value}";
                        }
                        else {
                            this.Add(key, value);
                        }
                    } 
                }
            }
        }

        public string TranslateKey(string keyName) {
            string s = keyName;
            switch (keyName) {
                case "CN": { s = "Nazwa"; break; }
                case "C": { s = "Kod kraju"; break; }
                case "ST": { s = "Województwo"; break; }
                case "L": { s = "Miejscowość"; break; }
                case "SERIALNUMBER": { s = "Identyfikator"; break; }
                case "O": { s = "Organizacja"; break; }
                case "SURNAME": { s = "Nazwisko"; break; }
                case "GIVENNAME": { s = "Imię"; break; }
                case "STREET": { s = "Ulica"; break; }
                case "OU": { s = "Jednostka organizacyjna"; break; }

                case "2.5.4.4": { s = "Nazwisko"; break; }
                case "2.5.4.42": { s = "Imiona"; break; }
                case "2.5.4.97":
                case "2.5.4.5": { s = "Identyfikator"; break; }
                case "2.5.4.16": { s = "Adres"; break; }
                case "2.5.4.17": { s = "Kod pocztowy"; break; }
            }
            return s;
        }
    }
}
