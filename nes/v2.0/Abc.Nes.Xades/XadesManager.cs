﻿using Abc.Nes.Xades.Crypto;
using Abc.Nes.Xades.Signature;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using Abc.Nes.Xades.Validation;
using Microsoft.Xades;
using Microsoft.XmlDsig;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace Abc.Nes.Xades {
    public interface IXadesManager : IDisposable {
        SignatureDocument AppendSignatureToXmlFile(Stream input, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null);
        SignatureDocument CreateEnvelopingSignature(Stream input, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null, string fileName = null);
        SignatureDocument CreateDetachedSignature(string filePath, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null);
        ValidationResult ValidateSignature(Stream stream);
        ValidationResult ValidateSignature(SignatureDocument sigDocument);
    }
    public class XadesManager : IXadesManager {
        private Signature.Parameters.SignatureProductionPlace ProductionPlace = null;
        private Signature.Parameters.SignerRole Role = null;
        private X509Certificate2 Certificate = null;
        private DataObjectFormat DataFormat = null;
        private Reference ContentReference = null;
        private Signer XadesSigner = null;

        public SignatureDocument AppendSignatureToXmlFile(Stream input, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null) {
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
                Description = @"
MIME-Version: 1.0
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

        public SignatureDocument CreateEnvelopingSignature(Stream input, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null, string fileName = null) {

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

            if (fileName != null) {
                DataFormat = new DataObjectFormat() {
                    MimeType = MimeTypeUtil.GetMimeType(fileName),
                    Encoding = "UTF-8",
                    Description = $@"
MIME-Version: 1.0
Content-Type: {MimeTypeUtil.GetMimeType(fileName)}
Content-Transfer-Encoding: binary
Content-Disposition: filename=""{ fileName }""
",
                    ObjectIdentifier = new ObjectIdentifier() {
                        Identifier = new Identifier() {
                            IdentifierUri = "http://www.certum.pl/OIDAsURI/signedFile/1.2.616.1.113527.3.1.1.3.1",
                            Qualifier = KnownQualifier.OIDAsURI
                        },
                        Description = "Description of the document format and its full name",
                        DocumentationReferences = new DocumentationReferences(new DocumentationReference() {
                            DocumentationReferenceUri = "http://www.certum.pl/OIDAsURI/signedFile.pdf"
                        })
                    }
                };
            }

            var objectXml = new XmlDocument { PreserveWhitespace = true };
            var objectXmlNode = objectXml.CreateElement("ds", "Object", "http://www.w3.org/2000/09/xmldsig#");
            var base64Node = objectXml.CreateTextNode(Convert.ToBase64String(input.ToArray()));
            objectXmlNode.AppendChild(base64Node);
            objectXml.AppendChild(objectXmlNode);

            if (objectXml.ChildNodes[0].NodeType == XmlNodeType.XmlDeclaration) {
                objectXml.RemoveChild(objectXml.ChildNodes[0]);
            }

            XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
            ContentReference.AddTransform(transform);
            signatureDocument.XadesSignature.AddReference(ContentReference);

            SetSignatureId(signatureDocument.XadesSignature);
            PrepareSignature(signatureDocument, false);

            signatureDocument.XadesSignature.AddObject(new DataObject() {
                Id = objectId,
                Data = objectXml.ChildNodes[0].ChildNodes,
                Encoding = "http://www.w3.org/2000/09/xmldsig#base64"
            });

            signatureDocument.XadesSignature.ComputeSignature();

            var xml = signatureDocument.XadesSignature.GetXml();
            signatureDocument.Document = xml.ToXmlDocument();

            UpdateXadesSignature(signatureDocument);

            return signatureDocument;
        }

        public SignatureDocument CreateDetachedSignature(string filePath, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null) {
            if (filePath == null) { throw new ArgumentNullException("filePath"); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException("Specified file not found!"); }
            ProductionPlace = productionPlace;
            Role = signerRole;
            Certificate = cert ?? throw new ArgumentNullException("cert");

            var signatureDocument = new SignatureDocument() {
                XadesSignature = new XadesSignedXml()
            };

            var fileName = Path.GetFileName(filePath);

            ContentReference = new Reference(filePath) {
                Id = $"FileReference-{Guid.NewGuid()}"
            };

            DataFormat = new DataObjectFormat() {
                MimeType = MimeTypeUtil.GetMimeType(fileName),
                Encoding = "UTF-8",
                Description = $@"
MIME-Version: 1.0
Content-Type: {MimeTypeUtil.GetMimeType(fileName)}
Content-Transfer-Encoding: binary
Content-Disposition: filename=""{ fileName }""
",
                ObjectIdentifier = new ObjectIdentifier() {
                    Identifier = new Identifier() {
                        IdentifierUri = "http://www.certum.pl/OIDAsURI/signedFile/1.2.616.1.113527.3.1.1.3.1",
                        Qualifier = KnownQualifier.OIDAsURI
                    },
                    Description = "Description of the document format and its full name",
                    DocumentationReferences = new DocumentationReferences(new DocumentationReference() {
                        DocumentationReferenceUri = "http://www.certum.pl/OIDAsURI/signedFile.pdf"
                    })
                }
            };

            //XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
            //ContentReference.AddTransform(transform);
            signatureDocument.XadesSignature.AddReference(ContentReference);

            SetSignatureId(signatureDocument.XadesSignature);
            PrepareSignature(signatureDocument, false);

            signatureDocument.XadesSignature.ComputeSignature();

            var xml = signatureDocument.XadesSignature.GetXml();
            signatureDocument.Document = xml.ToXmlDocument();

            UpdateXadesSignature(signatureDocument);

            return signatureDocument;
        }

        public ValidationResult ValidateSignature(Stream stream) {
            using (XadesValidator validator = new XadesValidator()) {
                return validator.Validate(stream);
            }
        }

        public ValidationResult ValidateSignature(SignatureDocument sigDocument) {
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

        private void PrepareSignature(SignatureDocument signatureDocument, bool addKeyInfoReference = true) {
            signatureDocument.XadesSignature.SignedInfo.SignatureMethod = SignatureMethod.RSAwithSHA256.URI;
            AddCertificateInfo(signatureDocument, addKeyInfoReference);
            AddXadesInfo(signatureDocument);
        }

        private void AddCertificateInfo(SignatureDocument signatureDocument, bool addKeyInfoReference = true) {
            XadesSigner = new Signer(Certificate);
            signatureDocument.XadesSignature.SigningKey = XadesSigner.SigningKey;

            var keyInfo = new KeyInfo {
                Id = "KeyInfoId-" + signatureDocument.XadesSignature.Signature.Id
            };
            keyInfo.AddClause(new KeyInfoX509Data((X509Certificate)XadesSigner.Certificate));
            keyInfo.AddClause(new RSAKeyValue((RSA)XadesSigner.SigningKey));

            signatureDocument.XadesSignature.KeyInfo = keyInfo;

            if (addKeyInfoReference) {
                var reference = new Reference {
                    Id = "ReferenceKeyInfo",
                    Uri = "#KeyInfoId-" + signatureDocument.XadesSignature.Signature.Id
                };

                signatureDocument.XadesSignature.AddReference(reference);
            }
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

            signedSignatureProperties.SigningTime = DateTime.Now;

            if (DataFormat != null) {
                var newDataObjectFormat = new DataObjectFormat {
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

            CommitmentTypeIndication cti = new CommitmentTypeIndication();
            cti.CommitmentTypeId.Identifier.IdentifierUri = SignatureCommitmentType.ProofOfApproval.URI;
            cti.AllSignedDataObjects = true;
            signedDataObjectProperties.CommitmentTypeIndicationCollection.Add(cti);

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
