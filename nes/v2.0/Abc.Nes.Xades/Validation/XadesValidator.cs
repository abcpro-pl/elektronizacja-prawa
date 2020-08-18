// --------------------------------------------------------------------------------------------------------------------
// XadesValidator.cs
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


using Abc.Nes.Xades.Signature;
using Abc.Nes.Xades.Utils;
using Microsoft.Xades;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace Abc.Nes.Xades.Validation {
    class XadesValidator : IDisposable {
        public void Dispose() { }
        public ValidationResult Validate(SignatureDocument sigDocument) {
            return Validate(sigDocument.XadesSignature);
        }

        public ValidationResult Validate(Stream stream) {
            if (stream == null) { throw new ArgumentNullException("stream"); }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(stream);
            var signature = new Microsoft.Xades.XadesSignedXml(xmlDocument);
            return Validate(signature);
        }

        public ValidationResult Validate(Microsoft.Xades.XadesSignedXml signature) {
            /* Los elementos que se validan son:
             * 
             * 1. Las huellas de las referencias de la firma.
             * 2. Se comprueba la huella del elemento SignedInfo y se verifica la firma con la clave pública del certificado.
             * 3. Si la firma contiene un sello de tiempo se comprueba que la huella de la firma coincide con la del sello de tiempo.
             * 
             * La validación de perfiles -C, -X, -XL y -A esta fuera del ámbito de este proyecto.
             */

            ValidationResult result = new ValidationResult();

            try {
                // Verifica las huellas de las referencias y la firma
                signature.CheckXmldsigSignature();
            }
            catch {
                result.IsValid = false;
                result.Message = "Signature verification was unsuccessful";

                return result;
            }

            if (signature.UnsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Count > 0) {
                // Se comprueba el sello de tiempo

                var timeStamp = signature.UnsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection[0];
                TimeStampToken token = new TimeStampToken(new CmsSignedData(timeStamp.EncapsulatedTimeStamp.PkiData));

                byte[] tsHashValue = token.TimeStampInfo.GetMessageImprintDigest();
                Crypto.DigestMethod tsDigestMethod = Crypto.DigestMethod.GetByOid(token.TimeStampInfo.HashAlgorithm.Algorithm.Id);

                Microsoft.XmlDsig.Transform transform = null;

                if (timeStamp.CanonicalizationMethod != null) {
                    transform = CryptoConfig.CreateFromName(timeStamp.CanonicalizationMethod.Algorithm) as Microsoft.XmlDsig.Transform;
                }
                else {
                    transform = new Microsoft.XmlDsig.XmlDsigC14NTransform();
                }

                ArrayList signatureValueElementXpaths = new ArrayList {
                    "ds:SignatureValue"
                };

                byte[] signatureValueHash = DigestUtil.ComputeHashValue(XMLUtil.ComputeValueOfElementList(signature, signatureValueElementXpaths, transform), tsDigestMethod);

                if (!Arrays.AreEqual(tsHashValue, signatureValueHash)) {
                    result.IsValid = false;
                    result.Message = "The trimestamp does not correspond to the calculated one";

                    return result;
                }
            }

            result.IsValid = true;
            result.Message = "Successful signature verification";

            return result;
        }

    }
}
