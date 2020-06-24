using Abc.Nes.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
            Assert.IsTrue(schema != null);
        }

        [TestMethod]
        public void Document_XmlConverter_WriteXml() {
            Abc.Nes.Document document = GetModel();
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            new Abc.Nes.Converters.XmlConverter().WriteXml(document, filePath);
            Assert.IsTrue(File.Exists(filePath));
        }

        [TestMethod]
        public void Document_XmlConverter_LoadXml() {
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            if (!File.Exists(filePath)) { Document_XmlConverter_WriteXml(); }
            var document = new Abc.Nes.Converters.XmlConverter().LoadXml(filePath);
            Assert.IsTrue(document != null && document.Groupings != null && document.Groupings.Count > 0);
        }


        [TestMethod]
        public void Document_XmlConverter_Validate() {
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            if (!File.Exists(filePath)) { Document_XmlConverter_WriteXml(); }
            var c = new Abc.Nes.Converters.XmlConverter();
            var valid = c.Validate(filePath);
            Assert.IsTrue(valid && c.ValidationErrors.Count == 0);
        }


        private Abc.Nes.Document GetModel() {
            var document = new Abc.Nes.Document() {
                Identifiers = new List<Abc.Nes.Elements.IdentifierElement> {
                    new Abc.Nes.Elements.IdentifierElement() {
                        Type = "Znak sprawy",
                        Value = "ABC-A.123.77.3.2011.JW.",
                        Subject = new Elements.SubjectElement(){
                            Institution = new Elements.InstitutionElement(){
                                Name = "Urząd Miasta Wołomierz"
                            }
                        }
                    }
                },
                Titles = new List<Abc.Nes.Elements.TitleElement> {
                    new Abc.Nes.Elements.TitleElement(){
                        Original = new Abc.Nes.Elements.TitleWithLanguageCodeElement(){
                            Type = Enumerations.LanguageCode.pol,
                            Value = "Tytuł dokumentu"
                        },
                        Alternative = new List<Elements.TitleWithLanguageCodeElement> {
                            new Elements.TitleWithLanguageCodeElement() {
                                Type = Enumerations.LanguageCode.eng,
                                Value = "Document title"
                            }
                        }
                    }
                },
                Dates = new List<Elements.DateElement> {
                    new Elements.DateElement() {
                        Type = Enumerations.EventDateType.Created,
                        Date = "2020-06-22"
                    }
                },
                Formats = new List<Elements.FormatElement> {
                    new Elements.FormatElement() {
                        Type = ".pdf",
                        Specification = "1.7",
                        Uncompleted = Enumerations.BooleanValues.False,
                        Size = new Elements.SizeElement(){
                            Measure = Elements.SizeElement.GetSizeType(Enumerations.FileSizeType.kB),
                            Value = "4712"
                        }
                    }
                },
                Access = new List<Elements.AccessElement> {
                    new Elements.AccessElement() {
                        Access = Enumerations.AccessType.Public,
                        Description = "Uwagi dotyczące dostępności",
                        Date = new Elements.AccessDateElement() {
                            Type = Enumerations.AccessDateType.After,
                            Date = "2020-06-23"
                        }
                    }
                },
                Types = new List<Elements.TypeElement>() {
                    new Elements.TypeElement() {
                        Class = Elements.TypeElement.GetDocumentClassType(Enumerations.DocumentClassType.Text),
                        Kinds = new List<string> { Elements.TypeElement.GetDocumentKindType(Enumerations.DocumentKindType.Document) }
                    }
                },
                Groupings = new List<Elements.GroupingElement> {
                    new Elements.GroupingElement() {
                        Type = "Rejestr korespondencji przychodzącej",
                        Code = "RKP01",
                        Description = "tekstowy opis grupy"
                    }
                },
                Description = "Opis dokumentu"
            };

            return document;
        }
    }
}
