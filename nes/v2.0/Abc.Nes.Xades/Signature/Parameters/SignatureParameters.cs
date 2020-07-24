// --------------------------------------------------------------------------------------------------------------------
// SignatureParameters.cs
//
// FirmaXadesNet - Librería para la generación de firmas XADES
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using Abc.Nes.Xades.Crypto;
using System;
using System.Collections.Generic;

namespace Abc.Nes.Xades.Signature.Parameters {
    public enum SignaturePackaging {
        INTERNALLY_DETACHED,
        HASH_INTERNALLY_DETACHED,
        EXTERNALLY_DETACHED,
        ENVELOPED,
        ENVELOPING
    }

    public class SignatureParameters {
        private SignatureMethod _defaultSignatureMethod = SignatureMethod.RSAwithSHA256;
        private DigestMethod _defaultDigestMethod = DigestMethod.SHA256;

        public Signer Signer { get; set; }
        public SignatureMethod SignatureMethod { get; set; }
        public DigestMethod DigestMethod { get; set; }
        public DateTime? SigningDate { get; set; }
        public SignerRole SignerRole { get; set; }
        public List<SignatureCommitment> SignatureCommitments { get; private set; }
        public SignatureProductionPlace SignatureProductionPlace { get; set; }
        public List<SignatureXPathExpression> XPathTransformations { get; private set; }
        public SignaturePolicyInfo SignaturePolicyInfo { get; set; }
        public SignatureXPathExpression SignatureDestination { get; set; }
        public SignaturePackaging SignaturePackaging { get; set; }
        public DataFormat DataFormat { get; set; }
        public string ElementIdToSign { get; set; }
        public string ExternalContentUri { get; set; }

        public string InternallyDetachedRootElementName { get; set; } = "DOCFIRMA";
        public string InternallyDetachedContentElementName { get; set; } = "CONTENT";
        public string InternallyDetachedContentId { get; set; }

        public SignatureParameters() {
            this.XPathTransformations = new List<SignatureXPathExpression>();
            this.SignatureCommitments = new List<SignatureCommitment>();
            this.SignatureMethod = _defaultSignatureMethod;
            this.DigestMethod = _defaultDigestMethod;
            this.InternallyDetachedContentId = "CONTENT-" + Guid.NewGuid().ToString();
        }
    }
}
