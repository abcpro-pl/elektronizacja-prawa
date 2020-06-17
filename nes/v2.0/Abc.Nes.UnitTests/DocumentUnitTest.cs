using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Abc.Nes.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class DocumentUnitTest {
        [TestMethod]
        public void Document_XsdGenerator_GetSchema() {
            XElement schema;
            using (var xsdGenerator = new XsdGenerator()) {
                schema = xsdGenerator.GetSchema();
            }
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xsd");
            schema.Save(filePath);
            Debug.Assert(schema != null);
        }
    }
}
