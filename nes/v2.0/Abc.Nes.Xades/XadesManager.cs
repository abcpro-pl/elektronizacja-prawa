using Abc.Nes.Xades.Crypto;
using Abc.Nes.Xades.Signature;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using Abc.Nes.Xades.Validation;
using Microsoft.Xades;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Abc.Nes.Xades {
    public class XadesManager : IDisposable {
        private Signature.Parameters.SignatureProductionPlace ProductionPlace = null;
        private Signature.Parameters.SignerRole Role = null;
        private X509Certificate2 Certificate = null;
        private DataObjectFormat DataFormat = null;
        private Reference ContentReference = null;
        private Signer XadesSigner = null;

        public SignatureDocument AppendSignatureToXmlFile(Stream input, X509Certificate2 cert,
            Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Signature.Parameters.SignerRole signerRole = null) {
            if (input == null) { throw new ArgumentNullException("input"); }

            ProductionPlace = productionPlace;
            Role = signerRole;
            Certificate = cert ?? throw new ArgumentNullException("cert");

            var signatureDocument = new SignatureDocument();
            try {
                signatureDocument.Document = XMLUtil.LoadDocument(input);
            }
            catch {
                throw new Exception("Input stream is not a valid XML file!");
            }

            signatureDocument.XadesSignature = new XadesSignedXml(signatureDocument.Document);

            DataFormat = new DataObjectFormat() {
                MimeType = "text/xml",
                Encoding = "UTF-8",
                Description = @"MIME-Version: 1.0
Content-Type: text/xml
Content-Transfer-Encoding: UTF-8"
            };

            ContentReference = new Reference {
                Id = "Reference-" + Guid.NewGuid().ToString(),
                Uri = ""
            };

            for (int i = 0; i < signatureDocument.Document.DocumentElement.Attributes.Count; i++) {
                if (signatureDocument.Document.DocumentElement.Attributes[i].Name.Equals("id", StringComparison.InvariantCultureIgnoreCase)) {
                    ContentReference.Uri = "#" + signatureDocument.Document.DocumentElement.Attributes[i].Value;
                    break;
                }
            }

            var xmlDsigEnvelopedSignatureTransform = new XmlDsigEnvelopedSignatureTransform();
            ContentReference.AddTransform(xmlDsigEnvelopedSignatureTransform);

            signatureDocument.XadesSignature.AddReference(ContentReference);

            SetSignatureId(signatureDocument.XadesSignature);
            PrepareSignature(signatureDocument);

            signatureDocument.XadesSignature.ComputeSignature();

            UpdateXadesSignature(signatureDocument);

            return signatureDocument;
        }

        public SignatureDocument CreateEnvelopingSignature(Stream input, X509Certificate2 cert,
            Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Signature.Parameters.SignerRole signerRole = null) {

            if (input == null) { throw new ArgumentNullException("input"); }

            ProductionPlace = productionPlace;
            Role = signerRole;
            Certificate = cert ?? throw new ArgumentNullException("cert");

            var signatureDocument = new SignatureDocument() {
                XadesSignature = new XadesSignedXml()
            };
            var objectId = $"DataObject-{Guid.NewGuid()}";
            ContentReference = new Reference() {
                Id = $"Reference-{Guid.NewGuid()}",
                Uri = $"#{objectId}",
                Type = XadesSignedXml.XmlDsigObjectType
            };

            var objectXml = new XmlDocument {
                PreserveWhitespace = true
            };
            var objectXmlNode = objectXml.CreateElement("ds", "Object", "http://www.w3.org/2000/09/xmldsig#");            
            var base64Node = objectXml.CreateTextNode(Convert.ToBase64String(input.ToArray()));
            objectXmlNode.AppendChild(base64Node);
            objectXml.AppendChild(objectXmlNode);

            if (objectXml.ChildNodes[0].NodeType == XmlNodeType.XmlDeclaration) {
                objectXml.RemoveChild(objectXml.ChildNodes[0]);
            }

            signatureDocument.XadesSignature.AddObject(new DataObject() {
                Id = objectId,
                Data = objectXml.ChildNodes[0].ChildNodes,
                Encoding = "http://www.w3.org/2000/09/xmldsig#base64"
            });

            XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
            ContentReference.AddTransform(transform);
            signatureDocument.XadesSignature.AddReference(ContentReference);

            SetSignatureId(signatureDocument.XadesSignature);
            PrepareSignature(signatureDocument);

            signatureDocument.XadesSignature.ComputeSignature();
                        
            var xml = signatureDocument.XadesSignature.GetXml();
            signatureDocument.Document = xml.ToXmlDocument();

            UpdateXadesSignature(signatureDocument);

            return signatureDocument;
        }

        public ValidationResult Validate(SignatureDocument sigDocument) {
            SignatureDocument.CheckSignatureDocument(sigDocument);

            using (XadesValidator validator = new XadesValidator()) {
                return validator.Validate(sigDocument);
            }
        }

        private void SetSignatureId(XadesSignedXml xadesSignedXml) {
            string id = Guid.NewGuid().ToString();
            xadesSignedXml.Signature.Id = "Signature-" + id;
            xadesSignedXml.SignatureValueId = "SignatureValue-" + id;
        }

        private void PrepareSignature(SignatureDocument signatureDocument) {
            signatureDocument.XadesSignature.SignedInfo.SignatureMethod = SignatureMethod.RSAwithSHA256.URI;
            AddCertificateInfo(signatureDocument);
            AddXadesInfo(signatureDocument);
        }

        private void AddCertificateInfo(SignatureDocument signatureDocument) {
            XadesSigner = new Signer(Certificate);
            signatureDocument.XadesSignature.SigningKey = XadesSigner.SigningKey;

            var keyInfo = new KeyInfo {
                Id = "KeyInfoId-" + signatureDocument.XadesSignature.Signature.Id
            };
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)XadesSigner.Certificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)XadesSigner.SigningKey));

            signatureDocument.XadesSignature.KeyInfo = keyInfo;

            var reference = new Reference {
                Id = "ReferenceKeyInfo",
                Uri = "#KeyInfoId-" + signatureDocument.XadesSignature.Signature.Id
            };

            signatureDocument.XadesSignature.AddReference(reference);
        }

        private void AddXadesInfo(SignatureDocument signatureDocument) {
            XadesObject xadesObject = new XadesObject {
                Id = "XadesObjectId-" + Guid.NewGuid().ToString()
            };
            xadesObject.QualifyingProperties.Id = "QualifyingProperties-" + Guid.NewGuid().ToString();
            xadesObject.QualifyingProperties.Target = "#" + signatureDocument.XadesSignature.Signature.Id;
            xadesObject.QualifyingProperties.SignedProperties.Id = "SignedProperties-" + signatureDocument.XadesSignature.Signature.Id;

            AddSignatureProperties(signatureDocument,
                xadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                xadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties);

            signatureDocument.XadesSignature.AddXadesObject(xadesObject);
        }

        private void AddSignatureProperties(SignatureDocument signatureDocument,
                    SignedSignatureProperties signedSignatureProperties,
                    SignedDataObjectProperties signedDataObjectProperties,
                    UnsignedSignatureProperties unsignedSignatureProperties
                    ) {


            var cert = new Cert();
            cert.IssuerSerial.X509IssuerName = XadesSigner.Certificate.IssuerName.Name;
            cert.IssuerSerial.X509SerialNumber = XadesSigner.Certificate.GetSerialNumberAsDecimalString();
            DigestUtil.SetCertDigest(XadesSigner.Certificate.GetRawCertData(), Crypto.DigestMethod.SHA256, cert.CertDigest);
            signedSignatureProperties.SigningCertificate.CertCollection.Add(cert);

            //if (parameters.SignaturePolicyInfo != null) {
            //    if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyIdentifier)) {
            //        signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyImplied = false;
            //        signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyId.Identifier.IdentifierUri = parameters.SignaturePolicyInfo.PolicyIdentifier;
            //    }

            //    if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyUri)) {
            //        SigPolicyQualifier spq = new SigPolicyQualifier();
            //        spq.AnyXmlElement = sigDocument.Document.CreateElement(XadesSignedXml.XmlXadesPrefix, "SPURI", XadesSignedXml.XadesNamespaceUri);
            //        spq.AnyXmlElement.InnerText = parameters.SignaturePolicyInfo.PolicyUri;

            //        signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyQualifiers.SigPolicyQualifierCollection.Add(spq);
            //    }

            //    if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyHash)) {
            //        signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestMethod.Algorithm = parameters.SignaturePolicyInfo.PolicyDigestAlgorithm.URI;
            //        signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestValue = Convert.FromBase64String(parameters.SignaturePolicyInfo.PolicyHash);
            //    }
            //}

            signedSignatureProperties.SigningTime = DateTime.Now;

            if (DataFormat != null) {
                DataObjectFormat newDataObjectFormat = new DataObjectFormat {
                    MimeType = DataFormat.MimeType,
                    Encoding = DataFormat.Encoding,
                    Description = DataFormat.Description,
                    ObjectReferenceAttribute = "#" + ContentReference.Id
                };

                if (DataFormat.ObjectIdentifier != null) {
                    newDataObjectFormat.ObjectIdentifier.Identifier.IdentifierUri = DataFormat.ObjectIdentifier.Identifier.IdentifierUri;
                }

                signedDataObjectProperties.DataObjectFormatCollection.Add(newDataObjectFormat);
            }

            if (Role != null && (Role.CertifiedRoles.Count > 0 || Role.ClaimedRoles.Count > 0)) {
                signedSignatureProperties.SignerRole = new Microsoft.Xades.SignerRole();

                foreach (X509Certificate certifiedRole in Role.CertifiedRoles) {
                    signedSignatureProperties.SignerRole.CertifiedRoles.CertifiedRoleCollection.Add(new CertifiedRole() { PkiData = certifiedRole.GetRawCertData() });
                }

                foreach (string claimedRole in Role.ClaimedRoles) {
                    signedSignatureProperties.SignerRole.ClaimedRoles.ClaimedRoleCollection.Add(new ClaimedRole() { InnerText = claimedRole });
                }
            }

            //if (parameters.SignerRole != null &&
            //    (parameters.SignerRole.CertifiedRoles.Count > 0 || parameters.SignerRole.ClaimedRoles.Count > 0)) {
            //    signedSignatureProperties.SignerRole = new Microsoft.Xades.SignerRole();

            //    foreach (X509Certificate certifiedRole in parameters.SignerRole.CertifiedRoles) {
            //        signedSignatureProperties.SignerRole.CertifiedRoles.CertifiedRoleCollection.Add(new CertifiedRole() { PkiData = certifiedRole.GetRawCertData() });
            //    }

            //    foreach (string claimedRole in parameters.SignerRole.ClaimedRoles) {
            //        signedSignatureProperties.SignerRole.ClaimedRoles.ClaimedRoleCollection.Add(new ClaimedRole() { InnerText = claimedRole });
            //    }
            //}

            CommitmentTypeIndication cti = new CommitmentTypeIndication();
            cti.CommitmentTypeId.Identifier.IdentifierUri = SignatureCommitmentType.ProofOfApproval.URI;
            cti.AllSignedDataObjects = true;
            signedDataObjectProperties.CommitmentTypeIndicationCollection.Add(cti);

            //foreach (SignatureCommitment signatureCommitment in parameters.SignatureCommitments) {
            //    CommitmentTypeIndication cti = new CommitmentTypeIndication();
            //    cti.CommitmentTypeId.Identifier.IdentifierUri = signatureCommitment.CommitmentType.URI;
            //    cti.AllSignedDataObjects = true;

            //    foreach (XmlElement signatureCommitmentQualifier in signatureCommitment.CommitmentTypeQualifiers) {
            //        var ctq = new CommitmentTypeQualifier {
            //            AnyXmlElement = signatureCommitmentQualifier
            //        };

            //        cti.CommitmentTypeQualifiers.CommitmentTypeQualifierCollection.Add(ctq);
            //    }

            //    signedDataObjectProperties.CommitmentTypeIndicationCollection.Add(cti);
            //}

            if (ProductionPlace != null) {
                signedSignatureProperties.SignatureProductionPlace.City = ProductionPlace.City;
                signedSignatureProperties.SignatureProductionPlace.StateOrProvince = ProductionPlace.StateOrProvince;
                signedSignatureProperties.SignatureProductionPlace.PostalCode = ProductionPlace.PostalCode;
                signedSignatureProperties.SignatureProductionPlace.CountryName = ProductionPlace.CountryName;
            }

        }

        private void UpdateXadesSignature(SignatureDocument signatureDocument) {
            signatureDocument.UpdateDocument();

            XmlElement signatureElement = (XmlElement)signatureDocument.Document.SelectSingleNode("//*[@Id='" + signatureDocument.XadesSignature.Signature.Id + "']");

            signatureDocument.XadesSignature = new XadesSignedXml(signatureDocument.Document);
            signatureDocument.XadesSignature.LoadXml(signatureElement);

            var upgrader = new Upgraders.XadesUpgraderService();
            upgrader.Upgrade(signatureDocument, Upgraders.SignatureFormat.XAdES_T, new Upgraders.Parameters.UpgradeParameters() {
                DigestMethod = Crypto.DigestMethod.SHA256,
                TimeStampClient = new Clients.TimeStampClient("http://time.certum.pl")
            });
        }

        public void Dispose() { }
    }
}
