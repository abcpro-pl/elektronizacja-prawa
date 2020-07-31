// OtherRef.cs
//
// XAdES Starter Kit for Microsoft .NET 3.5 (and above)
// 2010 Microsoft France
//
// Originally published under the CECILL-B Free Software license agreement,
// modified by Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
// and published under the GNU Lesser General Public License version 3.
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

using System.Xml;

namespace Microsoft.Xades {
    /// <summary>
    /// Alternative forms of validation data can be included in this class
    /// </summary>
    internal class OtherRef {
        #region Private variables
        private XmlElement anyXmlElement;
        #endregion

        #region Public properties
        /// <summary>
        /// The generic XML element that represents any other type of ref
        /// </summary>
        public XmlElement AnyXmlElement {
            get {
                return this.anyXmlElement;
            }
            set {
                this.anyXmlElement = value;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public OtherRef() {
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged() {
            bool retVal = false;

            if (this.anyXmlElement != null) {
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        /// Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(System.Xml.XmlElement xmlElement) {
            this.anyXmlElement = xmlElement;
        }

        /// <summary>
        /// Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public XmlElement GetXml() {
            XmlDocument creationXmlDocument;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("OtherRef", XadesSignedXml.XadesNamespaceUri);

            if (this.anyXmlElement != null) {
                retVal.AppendChild(creationXmlDocument.ImportNode(this.anyXmlElement, true));
            }

            return retVal;
        }
        #endregion
    }
}
