// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Security.Cryptography;

namespace Microsoft.XmlDsig {
    internal static class CryptoHelpers {
        private static readonly char[] _invalidChars = new char[] { ',', '`', '[', '*', '&' };

        public static object CreateFromKnownName(string name) {
            switch (name) {
                case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315": return new XmlDsigC14NTransform();
                case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments": return new XmlDsigC14NWithCommentsTransform();
                case "http://www.w3.org/2001/10/xml-exc-c14n#": return new XmlDsigExcC14NTransform();
                case "http://www.w3.org/2001/10/xml-exc-c14n#WithComments": return new XmlDsigExcC14NWithCommentsTransform();
                case "http://www.w3.org/2000/09/xmldsig#base64": return new XmlDsigBase64Transform();
                case "http://www.w3.org/TR/1999/REC-xpath-19991116": return new XmlDsigXPathTransform();
                case "http://www.w3.org/TR/1999/REC-xslt-19991116": return new XmlDsigXsltTransform();
                case "http://www.w3.org/2000/09/xmldsig#enveloped-signature": return new XmlDsigEnvelopedSignatureTransform();
                case "http://www.w3.org/2002/07/decrypt#XML": return new XmlDecryptionTransform();
                case "urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform": return new XmlLicenseTransform();
                case "http://www.w3.org/2000/09/xmldsig# X509Data": return new KeyInfoX509Data();
                case "http://www.w3.org/2000/09/xmldsig# KeyName": return new KeyInfoName();
                case "http://www.w3.org/2000/09/xmldsig# KeyValue/DSAKeyValue": return new DSAKeyValue();
                case "http://www.w3.org/2000/09/xmldsig# KeyValue/RSAKeyValue": return new RSAKeyValue();
                case "http://www.w3.org/2000/09/xmldsig# RetrievalMethod": return new KeyInfoRetrievalMethod();
                case "http://www.w3.org/2001/04/xmlenc# EncryptedKey": return new KeyInfoEncryptedKey();
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1": return new DSASignatureDescription();
                case "System.Security.Cryptography.DSASignatureDescription": return new DSASignatureDescription();
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1": return new RSAPKCS1SHA1SignatureDescription();
                case "System.Security.Cryptography.RSASignatureDescription": return new RSAPKCS1SHA1SignatureDescription();
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256": return new RSAPKCS1SHA256SignatureDescription();
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384": return new RSAPKCS1SHA384SignatureDescription();
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512": return new RSAPKCS1SHA512SignatureDescription();
                default: return null;
            }
        }

        public static T CreateFromName<T>(string name) where T : class {
            if (name == null || name.IndexOfAny(_invalidChars) >= 0) {
                return null;
            }
            try {
                var result = CreateFromKnownName(name); 
                if (result == null) {
                    result = CryptoConfig.CreateFromName(name);
                }
                return result as T;
                //return (CryptoConfig.CreateFromName(name) ?? CreateFromKnownName(name)) as T;
            }
            catch (Exception) {
                return null;
            }
        }
    }
}
