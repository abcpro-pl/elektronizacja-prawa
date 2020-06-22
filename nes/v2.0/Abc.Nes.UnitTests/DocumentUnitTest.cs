using Abc.Nes.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class DocumentUnitTest {
        [TestMethod]
        public void Document_XsdGenerator_GetSchema() {
            XElement schema;
            using (var xsdGenerator = new XsdGenerator()) {
                schema = xsdGenerator.GetSchema();
            }
            //var filePath = Path.Combine(Path.GetTempPath(), "nes.xsd");
            var filePath = @"..\..\..\nes_20_generated.xsd";
            schema.Save(filePath);
            Debug.Assert(schema != null);
        }

        [TestMethod]
        public void Document_XmlConverter_GetXml() {
            Abc.Nes.Document doc = new Abc.Nes.Document() {
                Identifiers = new List<Abc.Nes.Elements.IdentifierElement> {
                    new Abc.Nes.Elements.IdentifierElement() {
                        Type = "Znak sprawy",
                        Value = "ABC-A.123.77.3.2011.JW."
                    }
                },
                Titles = new List<Abc.Nes.Elements.TitleElement> {
                    new Abc.Nes.Elements.TitleElement(){
                        Original = new Abc.Nes.Elements.TitleWithLanguageCodeElement(){
                            Type = Enumerations.LanguageCode.pol,
                            Value = "Tytuł dokumentu"
                        }
                    }
                },
                Description = "Opis dokumentu"
            };
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            new Abc.Nes.Converters.XmlConverter().WriteXml(doc, filePath);
            Debug.Assert(File.Exists(filePath));
        }
    }
}
