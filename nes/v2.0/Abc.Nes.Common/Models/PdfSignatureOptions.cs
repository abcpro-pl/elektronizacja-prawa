using System;
using System.Collections.Generic;
using Abc.Nes.Common;

namespace Abc.Nes.Common.Models {
    public class PdfSignatureOptions : SignatureOptions{
        public bool AddVisibleSignature { get; set; }
        public bool ImageAsBackground { get; set; }
        public bool AllowMultipleSignatures { get; set; }
        public byte[] SignatureImage { get; set; }
        public PdfSignatureLocation SignatureLocation { get; set; } = PdfSignatureLocation.Custom;

        public float PositionX { get; set; } = 30;
        public float PositionY { get; set; } = 650;
        public float Width { get; set; } = 150;
        public float Height { get; set; } = 40;
        public float Margin { get; set; } = 10;

    }
}
