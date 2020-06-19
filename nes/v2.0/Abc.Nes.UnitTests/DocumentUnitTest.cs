using Abc.Nes.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
