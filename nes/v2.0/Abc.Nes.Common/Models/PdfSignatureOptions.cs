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

        public int PageNumber { get; set; } = 1;
        public float FontSize { get; set; } = 6;
        public float PositionX { get; set; } = 30;
        public float PositionY { get; set; } = 650;
        public float Width { get; set; } = 140;
        public float Height { get; set; } = 40;
        public float Margin { get; set; } = 1;

        public string SignatureTitle { get; set; } = "Elektronicznie podpisany przez:";
        public string ReasonText { get; set; } = "Rodzaj: ";
        public string LocationText { get; set; } = "Miejsce: ";

        public string DatePrefix { get; set; } = "dnia";
        public string DatePostfix { get; set; } = "r.";
        public string DateCultureInfoName { get; set; } = "pl-PL";
        public string DateStringFormat { get; set; } = "{3} {0} {1} {2} {4}";
    }
}
