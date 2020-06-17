using System.IO;
using System.Text;

namespace Abc.Nes.Utils {
    class Utf8StringWriter : StringWriter {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
