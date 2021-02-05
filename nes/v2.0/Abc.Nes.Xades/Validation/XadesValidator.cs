//// --------------------------------------------------------------------------------------------------------------------
//// XadesValidator.cs
////
//// FirmaXadesNet - Librería para la generación de firmas XADES
//// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
////
//// This program is free software: you can redistribute it and/or modify
//// it under the +terms of the GNU Lesser General Public License as published by
//// the Free Software Foundation, either version 3 of the License, or
//// (at your option) any later version.
////
//// This program is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU Lesser General Public License for more details.
////
//// You should have received a copy of the GNU Lesser General Public License
//// along with this program.  If not, see http://www.gnu.org/licenses/. 
////
//// E-Mail: informatica@gemuc.es
//// 
//// --------------------------------------------------------------------------------------------------------------------


//using Abc.Nes.Xades.Signature;
//using Abc.Nes.Xades.Utils;
//using Org.BouncyCastle.Cms;
//using Org.BouncyCastle.Tsp;
//using Org.BouncyCastle.Utilities;
//using System;
//using System.Collections;
//using System.IO;
//using System.Security.Cryptography;
//using System.Xml;

//namespace Abc.Nes.Xades.Validation {
//    class XadesValidator : IDisposable {
//        public void Dispose() { }

//        public ValidationResult Validate(string filePath) {
//            if (filePath == null) { throw new ArgumentNullException("filePath"); }
//            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

//            var xmlDocument = new XmlDocument();
//            xmlDocument.Load(filePath);

//            var signature = new Microsoft.Xades.XadesSignedXml(xmlDocument);
//            try {
//                signature.LoadXml(xmlDocument.DocumentElement);
//            }
//            catch {
//                return default;
//            }
//            return Validate(signature);
//        }


//        public ValidationResult Validate(SignatureDocument sigDocument) {
//            return Validate(sigDocument.XadesSignature);
//        }

//        public ValidationResult Validate(Stream stream) {
//            if (stream == null) { throw new ArgumentNullException("stream"); }

//            var xmlDocument = new XmlDocument();
//            xmlDocument.Load(stream);
//            var signature = new Microsoft.Xades.XadesSignedXml(xmlDocument);
//            signature.LoadXml(xmlDocument.DocumentElement);
//            return Validate(signature);
//        }

//        public ValidationResult Validate(Microsoft.Xades.XadesSignedXml signature) {
//            //Elementy, które są sprawdzane to:
//            // 1.Ślady referencji firmy.
//            // 2.Sprawdzany jest odcisk palca elementu SignedInfo, a podpis weryfikowany kluczem publicznym certyfikatu.
//            // 3.Jeśli podpis zawiera znacznik czasu, sprawdza się, czy wydruk podpisu jest zgodny z wydrukiem znacznika czasu.
//            //Walidacja profili -C, -X, -XL i - A wykracza poza zakres tego projektu.

//            ValidationResult result = new ValidationResult();

//            try {
//                var cert = signature.GetSigningCertificate();
//                result.CertificateIsValid = ValidateCert(cert);
//            }
//            catch { }

//            try {
//                // Sprawdź odcisk palca referencji i podpis 
//                signature.CheckXmldsigSignature();
//            }
//            catch {
//                result.IsValid = false;
//                result.SignatureName = signature.Signature?.Id;
//                result.Message = "Signature verification was unsuccessful";

//                return result;
//            }

//            if (signature.UnsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Count > 0) {
//                // Sprawdzanie znacznika czasu

//                var timeStamp = signature.UnsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection[0];
//                var token = new TimeStampToken(new CmsSignedData(timeStamp.EncapsulatedTimeStamp.PkiData));

//                byte[] tsHashValue = token.TimeStampInfo.GetMessageImprintDigest();
//                Crypto.DigestMethod tsDigestMethod = Crypto.DigestMethod.GetByOid(token.TimeStampInfo.HashAlgorithm.Algorithm.Id);

//                Microsoft.XmlDsig.Transform transform;
//                if (timeStamp.CanonicalizationMethod != null) {
//                    transform = CryptoConfig.CreateFromName(timeStamp.CanonicalizationMethod.Algorithm) as Microsoft.XmlDsig.Transform;
//                    if (transform == null) {
//                        transform = new Microsoft.XmlDsig.XmlDsigC14NTransform();
//                    }
//                }
//                else {
//                    transform = new Microsoft.XmlDsig.XmlDsigC14NTransform();
//                }

//                ArrayList signatureValueElementXpaths = new ArrayList {
//                    "ds:SignatureValue"
//                };

//                byte[] signatureValueHash = DigestUtil.ComputeHashValue(XMLUtil.ComputeValueOfElementList(signature, signatureValueElementXpaths, transform), tsDigestMethod);

//                if (!Arrays.AreEqual(tsHashValue, signatureValueHash)) {
//                    result.IsValid = false;
//                    result.SignatureName = signature.Signature?.Id;
//                    result.Message = "The timestamp does not correspond to the calculated one";

//                    return result;
//                }
//            }

//            result.IsValid = true;
//            result.SignatureName = signature.Signature?.Id;
//            result.Message = "Successful signature verification";

//            return result;
//        }
//        private bool ValidateCert(System.Security.Cryptography.X509Certificates.X509Certificate2 e) {
//            if (e != null) {
//                var ch = new System.Security.Cryptography.X509Certificates.X509Chain();
//                ch.ChainPolicy.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
//                if (ch.Build(e)) { return true; }
//            }
//            return default;
//        }
//    }
//}
