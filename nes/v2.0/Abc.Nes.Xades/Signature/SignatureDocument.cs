// --------------------------------------------------------------------------------------------------------------------
// SignatureDocument.cs
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
// Modified by ITORG Krzysztof Radzimski
// --------------------------------------------------------------------------------------------------------------------

using Abc.Nes.Xades.Utils;
using Microsoft.Xades;
using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Abc.Nes.Xades.Signature {
    public class SignatureDocument {
        public XmlDocument Document { get; set; }
        public XadesSignedXml XadesSignature { get; set; }

        public byte[] GetDocumentBytes() {
            CheckSignatureDocument(this);

            using (MemoryStream ms = new MemoryStream()) {
                Save(ms);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Guardar la firma en el fichero especificado.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName) {
            CheckSignatureDocument(this);

            var settings = new XmlWriterSettings {
                Encoding = new UTF8Encoding()
            };
            using (var writer = XmlWriter.Create(fileName, settings)) {
                this.Document.Save(writer);
            }
        }

        /// <summary>
        /// Guarda la firma en el destino especificado
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output) {
            var settings = new XmlWriterSettings {
                Encoding = new UTF8Encoding()
            };
            using (var writer = XmlWriter.Create(output, settings)) {
                this.Document.Save(writer);
            }
        }


        /// <summary>
        /// Actualiza el documento resultante
        /// </summary>
        internal void UpdateDocument() {
            if (Document == null) {
                Document = new XmlDocument();
            }

            if (Document.DocumentElement != null) {
                XmlNode xmlNode = Document.SelectSingleNode("//*[@Id='" + XadesSignature.Signature.Id + "']");

                if (xmlNode != null) {

                    XmlNamespaceManager nm = new XmlNamespaceManager(Document.NameTable);
                    nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
                    nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

                    XmlNode xmlQPNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties", nm);
                    XmlNode xmlUnsingedPropertiesNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties", nm);

                    if (xmlUnsingedPropertiesNode != null) {
                        XmlNode xmlUnsingedSignaturePropertiesNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties", nm);
                        XmlElement xmlUnsignedPropertiesNew = XadesSignature.XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties.GetXml();
                        foreach (XmlNode childNode in xmlUnsignedPropertiesNew.ChildNodes) {
                            if (childNode.Attributes["Id"] != null &&
                                xmlUnsingedSignaturePropertiesNode.SelectSingleNode("//*[@Id='" + childNode.Attributes["Id"].Value + "']") == null) {
                                var newNode = Document.ImportNode(childNode, true);
                                xmlUnsingedSignaturePropertiesNode.AppendChild(newNode);
                            }
                        }

                        // Se comprueban las ContraFirmas
                        if (XadesSignature.XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties.CounterSignatureCollection.Count > 0) {
                            foreach (XadesSignedXml counterSign in XadesSignature.XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties.CounterSignatureCollection) {
                                if (xmlNode.SelectSingleNode("//*[@Id='" + counterSign.Signature.Id + "']") == null) {
                                    XmlNode xmlCounterSignatureNode = Document.CreateElement(XadesSignedXml.XmlXadesPrefix, "CounterSignature", XadesSignedXml.XadesNamespaceUri);
                                    xmlUnsingedSignaturePropertiesNode.AppendChild(xmlCounterSignatureNode);
                                    xmlCounterSignatureNode.AppendChild(Document.ImportNode(counterSign.GetXml(), true));
                                }
                            }
                        }
                    }
                    else {
                        xmlUnsingedPropertiesNode = Document.ImportNode(XadesSignature.XadesObject.QualifyingProperties.UnsignedProperties.GetXml(), true);
                        xmlQPNode.AppendChild(xmlUnsingedPropertiesNode);
                    }

                }
                else {
                    XmlElement xmlSigned = XadesSignature.GetXml();

                    byte[] canonicalizedElement = XMLUtil.ApplyTransform(xmlSigned, new XmlDsigC14NTransform());

                    XmlDocument doc = new XmlDocument();
                    doc.PreserveWhitespace = true;
                    doc.LoadXml(Encoding.UTF8.GetString(canonicalizedElement));

                    XmlNode canonSignature = Document.ImportNode(doc.DocumentElement, true);

                    XadesSignature.GetSignatureElement().AppendChild(canonSignature);
                }
            }
            else {
                Document.LoadXml(XadesSignature.GetXml().OuterXml);
            }
        }


        internal static void CheckSignatureDocument(SignatureDocument sigDocument) {
            if (sigDocument == null) {
                throw new ArgumentNullException("sigDocument");
            }

            if (sigDocument.Document == null || sigDocument.XadesSignature == null) {
                throw new Exception("Signatures not found!");
            }
        }        
    }
}
