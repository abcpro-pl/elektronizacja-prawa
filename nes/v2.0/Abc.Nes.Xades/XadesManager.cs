using Abc.Nes.Common;
using Abc.Nes.Common.Models;
using Abc.Nes.Cryptography;
using Abc.Nes.Xades.Crypto;
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
        SignatureDocument AppendSignatureToXmlFile(Stream input,
                                                   X509Certificate2 cert,
                                                   Signature.Parameters.SignatureProductionPlace productionPlace = null,
                                                   Signature.Parameters.SignerRole signerRole = null,
                                                   Upgraders.SignatureFormat? upgradeFormat = null,
                                                   string timeStampServerUrl = "http://time.certum.pl",
                                                   CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval,
                                                   string timeStampPolicy = null,
                                                   X509Certificate2 timeStampCertificate = null,
                                                   string timeStampLogin = null,
                                                   string timeStampPassword = null);
        SignatureDocument CreateEnvelopingSignature(Stream input, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null, string fileName = null, Upgraders.SignatureFormat? upgradeFormat = null, string timeStampServerUrl = "http://time.certum.pl", CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval);
        SignatureDocument CreateDetachedSignature(string filePath, X509Certificate2 cert, Signature.Parameters.SignatureProductionPlace productionPlace = null, Signature.Parameters.SignerRole signerRole = null, Upgraders.SignatureFormat? upgradeFormat = null, string timeStampServerUrl = "http://time.certum.pl", CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval);
        ValidationResult ValidateSignature(Stream stream);
        ValidationResult ValidateSignature(string filePath);
    }
    public class XadesManager : IXadesManager {
        private Signature.Parameters.SignatureProductionPlace ProductionPlace = null;
        private Signature.Parameters.SignerRole Role = null;
        private X509Certificate2 Certificate = null;
        private DataObjectFormat DataFormat = null;
        private Reference ContentReference = null;
        private Signer XadesSigner = null;

        public void SignXmlFile(string filePath, SignatureOptions opts, string outputPath) {
            if (File.Exists(filePath)) {
                byte[] fileBytes = File.ReadAllBytes(filePath);

                var fileStream = new MemoryStream(fileBytes);

                Upgraders.SignatureFormat? upgradeType = null;

                if (opts.AddTimestamp)
                    upgradeType = Upgraders.SignatureFormat.XAdES_T;

                SignatureDocument result = AppendSignatureToXmlFile(
                    fileStream, opts.Certificate,
                    null, null,
                    upgradeType,
                    opts.TimestampOptions?.TsaUrl,
                    opts.Reason,
                    opts.TimestampOptions?.TsaPolicy,
                    opts.TimestampOptions?.Certificate,
                    opts.TimestampOptions?.Login,
                    opts.TimestampOptions?.Password
                );

                if (result != null) {
                    result.Save(outputPath);
                }
            }
        }

        public SignatureDocument AppendSignatureToXmlFile(
            Stream input,
            X509Certificate2 cert,
            Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Signature.Parameters.SignerRole signerRole = null,
            Upgraders.SignatureFormat? upgradeFormat = null,
            string timeStampServerUrl = "http://time.certum.pl",
            CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval,
            string timeStampPolicy = null,
            X509Certificate2 timeStampCertificate = null,
            string timeStampLogin = null,
            string timeStampPassword = null) {

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
            PrepareSignature(signatureDocument, commitmentTypeId: commitmentTypeId);

            signatureDocument.XadesSignature.ComputeSignature();
            
            UpdateXadesSignature(signatureDocument, upgradeFormat, timeStampServerUrl, timeStampPolicy, timeStampLogin, timeStampPassword, timeStampCertificate);

            return signatureDocument;
        }

        public SignatureDocument CreateEnvelopingSignature(
            Stream input,
            X509Certificate2 cert,
            Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Signature.Parameters.SignerRole signerRole = null,
            string fileName = null,
            Upgraders.SignatureFormat? upgradeFormat = null,
            string timeStampServerUrl = "http://time.certum.pl",
            CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {

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
            PrepareSignature(signatureDocument, false, commitmentTypeId);

            signatureDocument.XadesSignature.AddObject(new DataObject() {
                Id = objectId,
                Data = objectXml.ChildNodes[0].ChildNodes,
                Encoding = "http://www.w3.org/2000/09/xmldsig#base64"
            });

            signatureDocument.XadesSignature.ComputeSignature();

            var xml = signatureDocument.XadesSignature.GetXml();
            signatureDocument.Document = xml.ToXmlDocument();

            UpdateXadesSignature(signatureDocument, upgradeFormat, timeStampServerUrl);

            return signatureDocument;
        }

        public SignatureDocument CreateDetachedSignature(
            string filePath,
            X509Certificate2 cert,
            Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Signature.Parameters.SignerRole signerRole = null,
            Upgraders.SignatureFormat? upgradeFormat = null,
            string timeStampServerUrl = "http://time.certum.pl",
            CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {

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
            PrepareSignature(signatureDocument, false, commitmentTypeId);

            signatureDocument.XadesSignature.ComputeSignature();

            var xml = signatureDocument.XadesSignature.GetXml();
            signatureDocument.Document = xml.ToXmlDocument();

            UpdateXadesSignature(signatureDocument, upgradeFormat, timeStampServerUrl);

            return signatureDocument;
        }

        public ValidationResult ValidateSignature(Stream stream) {
            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;
            xd.Load(stream);
            var signedXml = new SignedXml(xd);

            XmlNode MessageSignatureNode = xd.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0];
            if (MessageSignatureNode == null) {
                return null;
            }

            signedXml.LoadXml((XmlElement)MessageSignatureNode);

            // get the cert from the signature
            X509Certificate2 certificate = null;
            foreach (KeyInfoClause clause in signedXml.KeyInfo) {
                if (clause is KeyInfoX509Data) {
                    if (((KeyInfoX509Data)clause).Certificates.Count > 0) {
                        certificate =
                        (X509Certificate2)((KeyInfoX509Data)clause).Certificates[0];
                    }
                }
            }

            var isValid = false;
            var message = String.Empty;
            CertificateValidationInfo certValidationInfo = null;
            try {
                certValidationInfo = ValidateCert(certificate);
                message = certValidationInfo.ErrorMessage;
                isValid = signedXml.CheckSignature(certificate, true);
                if (String.IsNullOrEmpty(message)) {
                    if (!isValid && certValidationInfo.IsValid) {
                        message = "Weryfikacja sygnatury podpisu zakończona niepowodzeniem.";
                    }
                    else if (isValid && !certValidationInfo.IsValid) {
                        message = "Weryfikacja certyfikatu osoby podpisującej zakończona niepowodzeniem.";
                    }
                    else if (!isValid && !certValidationInfo.IsValid) {
                        message = "Weryfikacja podpisu i certyfikatu zakończona niepowodzeniem.";
                    }
                    else {
                        message = "Weryfikacja sygnatury podpisu i certyfikatu przebiegła pomyślnie.";
                    }
                }
                else if (!String.IsNullOrEmpty(message) && !isValid) {
                    message = $"Weryfikacja sygnatury podpisu zakończona niepowodzeniem. {message}";
                }
            }
            catch (CryptographicException ex) {
                message = ex.Message;
            }

            return new ValidationResult() {
                IsValid = isValid,
                CertValidationInfo = certValidationInfo,
                Message = message,
                SignatureName = signedXml.Signature?.Id
            };
        }

        public ValidationResult ValidateSignature(string filePath) {
            byte[] stringData = File.ReadAllBytes(filePath);
            using (MemoryStream ms = new MemoryStream(stringData)) {
                System.Environment.CurrentDirectory = Path.GetDirectoryName(filePath);
                return ValidateSignature(ms);
            }
        }

        private CertificateValidationInfo ValidateCert(X509Certificate2 e) {
            if (e != null) {
                var ch = new X509Chain();
                ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                if (ch.Build(e)) { return new CertificateValidationInfo(true); }
                else {

                    foreach (X509ChainStatus chainStatus in ch.ChainStatus) {
                        var userName = GetSubjectCommonName(e);
                        return new CertificateValidationInfo(chainStatus, userName);
                    }
                }
            }
            return default;
        }

        private string GetSubjectCommonName(X509Certificate2 e) {
            var sc = e.Subject.Split(',');
            if (sc.Length > 0) {
                foreach (string t in sc) {
                    string[] sci = t.Split('=');
                    if (sci.Length > 1) {
                        if (sci[0].Trim() == "CN") {
                            return sci[1].Trim();
                        }
                    }
                }
            }
            return String.Empty;
        }

        private void SetSignatureId(XadesSignedXml xadesSignedXml) {
            string id = Guid.NewGuid().ToString();
            xadesSignedXml.Signature.Id = "Signature-" + id;
            xadesSignedXml.SignatureValueId = "SignatureValue-" + id;
        }

        private void PrepareSignature(SignatureDocument signatureDocument, bool addKeyInfoReference = true, CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {
            signatureDocument.XadesSignature.SignedInfo.SignatureMethod = SignatureMethod.RSAwithSHA256.URI;
            AddCertificateInfo(signatureDocument, addKeyInfoReference);
            AddXadesInfo(signatureDocument, commitmentTypeId);
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

        private void AddXadesInfo(SignatureDocument signatureDocument, CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {
            XadesObject xadesObject = new XadesObject {
                Id = "XadesObjectId-" + Guid.NewGuid().ToString()
            };
            xadesObject.QualifyingProperties.Id = "QualifyingProperties-" + Guid.NewGuid().ToString();
            xadesObject.QualifyingProperties.Target = "#" + signatureDocument.XadesSignature.Signature.Id;
            xadesObject.QualifyingProperties.SignedProperties.Id = "SignedProperties-" + signatureDocument.XadesSignature.Signature.Id;

            AddSignatureProperties(signatureDocument,
                xadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                xadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties,
                commitmentTypeId);

            signatureDocument.XadesSignature.AddXadesObject(xadesObject);
        }

        private void AddSignatureProperties(SignatureDocument signatureDocument,
                    SignedSignatureProperties signedSignatureProperties,
                    SignedDataObjectProperties signedDataObjectProperties,
                    UnsignedSignatureProperties unsignedSignatureProperties,
                    CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval
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
            cti.CommitmentTypeId.Identifier.IdentifierUri = new SignatureCommitmentType(commitmentTypeId).URI;
            cti.AllSignedDataObjects = true;
            signedDataObjectProperties.CommitmentTypeIndicationCollection.Add(cti);

            if (ProductionPlace != null) {
                signedSignatureProperties.SignatureProductionPlace.City = ProductionPlace.City;
                signedSignatureProperties.SignatureProductionPlace.StateOrProvince = ProductionPlace.StateOrProvince;
                signedSignatureProperties.SignatureProductionPlace.PostalCode = ProductionPlace.PostalCode;
                signedSignatureProperties.SignatureProductionPlace.CountryName = ProductionPlace.CountryName;
            }
        }

        private void UpdateXadesSignature(SignatureDocument signatureDocument,
                                          Upgraders.SignatureFormat? upgradeFormat = null,
                                          string timeStampServerUrl = "http://time.certum.pl",
                                          string timeStampReqPolicy = null,
                                          string timeStampLogin = null,
                                          string timeStampPassword = null,
                                          X509Certificate2 timeStampCertificate = null) {

                signatureDocument.UpdateDocument();

                XmlElement signatureElement = (XmlElement)signatureDocument.Document.SelectSingleNode("//*[@Id='" + signatureDocument.XadesSignature.Signature.Id + "']");

                signatureDocument.XadesSignature = new XadesSignedXml(signatureDocument.Document);
                signatureDocument.XadesSignature.LoadXml(signatureElement);

            if (upgradeFormat.HasValue) {
                var upgrader = new Upgraders.XadesUpgraderService();

                Clients.TimeStampClient timeStampClient;
                if (timeStampCertificate != null)
                    timeStampClient = new Clients.TimeStampClient(timeStampServerUrl, timeStampCertificate, timeStampReqPolicy);
                else if (timeStampLogin != null)
                    timeStampClient = new Clients.TimeStampClient(timeStampServerUrl, timeStampLogin, timeStampPassword, timeStampReqPolicy);
                else
                    timeStampClient = new Clients.TimeStampClient(timeStampServerUrl, timeStampReqPolicy);

                upgrader.Upgrade(signatureDocument, upgradeFormat.Value, new Upgraders.Parameters.UpgradeParameters() {
                    DigestMethod = Crypto.DigestMethod.SHA256,
                    TimeStampClient = timeStampClient
                });
            }
        }

        public void Dispose() { }
    }
}
