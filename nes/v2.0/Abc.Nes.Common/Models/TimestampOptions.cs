using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Abc.Nes.Common.Models {
    public class TimestampOptions {
        public string TsaUrl { get; set; }
        public string TsaPolicy { get; set; } = null;
        public X509Certificate2 Certificate { get; set; } = null;
        public string Login { get; set; } = null;
        public string Password { get; set; } = null;
    }
}
