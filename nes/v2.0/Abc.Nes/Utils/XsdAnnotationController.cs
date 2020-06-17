/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Abc.Nes.Utils {
    public class XsdAnnotationController : IDisposable {
        readonly List<Type> Types = null;
        private XsdAnnotationController() { }
        public XsdAnnotationController(params string[] namespaces) : this() {
            var asm = Assembly.GetCallingAssembly();
            Type[] allTypes = null;
            try {
                allTypes = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex) {
                allTypes = ex.Types;
            }
            if (allTypes != null) {
                if (namespaces.IsNotNull() && namespaces.Length > 0) {
                    Types = allTypes.Where(t => (t.IsClass || t.IsEnum) && t.FullName.ContainsInTable(true, false, namespaces)).ToList();
                }
                else {
                    Types = allTypes.Where(t => t.IsClass || t.IsEnum).ToList();
                }
            }
        }
        public XElement AddAnnotations(XElement xsd, bool addPropertiesAnnotation = true) {
            if (xsd.IsNotNull() && xsd.HasElements) {

                // Add annotations for classes
                var xsdTypes = xsd.Elements().Where(x => (x.Name.LocalName == "complexType" || x.Name.LocalName == "simpleType") && x.Attribute("name").IsNotNull() && x.Attribute("name").Value.IsNotNullOrEmpty());
                foreach (var xsdType in xsdTypes) {
                    var typeName = xsdType.Attribute("name").Value;
                    var type = GetClassType(typeName);
                    var annotation = GetTypeAnnotation(type);
                    if (annotation.IsNotNullOrEmpty() && xsdType.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                        xsdType.AddFirst(new XElement(XName.Get("annotation", xsdType.Name.NamespaceName),
                            new XElement(XName.Get("documentation", xsdType.Name.NamespaceName),
                                new XText(annotation)
                            )
                        ));

                        var elements = xsd.Descendants().Where(x => x.Attribute("type").IsNotNull() && (x.Attribute("type").Value == typeName || x.Attribute("type").Value.EndsWith($":{typeName}")));
                        foreach (var element in elements) {
                            if (element.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                                element.AddFirst(new XElement(XName.Get("annotation", element.Name.NamespaceName),
                                    new XElement(XName.Get("documentation", element.Name.NamespaceName),
                                        new XText(annotation)
                                    )
                                ));
                            }
                        }
                    }
                }

                // Add annotations for properties
                foreach (var xsdType in xsdTypes) {
                    var typeName = xsdType.Attribute("name").Value;
                    var type = GetClassType(typeName);

                    if (addPropertiesAnnotation && type.IsNotNull()) {
                        var xsdElements = xsdType.Descendants().Where(x => (x.Name.LocalName == "element" || x.Name.LocalName == "attribute") && x.Attribute("name") != null);
                        var properties = type.GetProperties();
                        foreach (var property in properties) {
                            var propertyName = GetPropertyXmlName(property);
                            var propertyAnnotation = GetPropertyAnnotation(property);
                            if (propertyAnnotation.IsNotNullOrEmpty()) {
                                var xsdElementItems = xsdElements.Where(x => x.Attribute("name").Value == propertyName);
                                foreach (var xsdElement in xsdElementItems) {
                                    if (xsdElement.IsNotNull()) {
                                        if (xsdElement.HasElements && xsdElement.Elements().First().Name.LocalName == "annotation") {
                                            xsdElement.Elements().First().Remove(); // remove existing annotation
                                        }
                                        if (xsdElement.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                                            xsdElement.AddFirst(new XElement(XName.Get("annotation", xsdElement.Name.NamespaceName),
                                                new XElement(XName.Get("documentation", xsdElement.Name.NamespaceName),
                                                    new XText(propertyAnnotation)
                                                )
                                            ));
                                        }


                                        if (xsdElement.Attribute("type").IsNotNull()) {
                                            var xsdElementType = xsdElement.Attribute("type").Value;
                                            if (xsdElementType.Contains(":")) { xsdElementType = xsdElementType.Substring(xsdElementType.IndexOf(":") + 1); }

                                            var _elementType = xsdTypes.Where(x => x.Attribute("name").Value == xsdElementType).FirstOrDefault();
                                            if (_elementType.IsNotNull() && _elementType.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                                                _elementType.AddFirst(new XElement(XName.Get("annotation", xsdElement.Name.NamespaceName),
                                                   new XElement(XName.Get("documentation", xsdElement.Name.NamespaceName),
                                                       new XText(propertyAnnotation)
                                                   )
                                               ));
                                            }
                                        }
                                    }
                                }
                            }


                            var required = GetPropertyRequired(property);
                            if (required) {
                                var xsdElementItems = xsdElements.Where(x => x.Attribute("name").Value == propertyName);
                                if (xsdElementItems.IsNotNull() && xsdElementItems.Count() > 0 && xsdElementItems.First().Parent.Name.LocalName == "choice") {
                                    xsdElementItems = xsdElementItems.First().Parent.Elements();
                                }
                                foreach (var xsdElement in xsdElementItems) {
                                    if (xsdElement.Attribute("minOccurs").IsNotNull()) {
                                        xsdElement.Attribute("minOccurs").Remove();
                                    }
                                }
                            }

                            var simpleType = GetPropertySimpleType(property);
                            if (simpleType.IsNotNull()) {

                                var simpleTypeElement = xsd.Elements().Where(x => x.Name.LocalName == "simpleType" && x.Attribute("name").Value == simpleType.TypeName).FirstOrDefault();
                                if (simpleTypeElement.IsNull()) {
                                    simpleTypeElement = new XElement(N("simpleType"),
                                        new XAttribute("name", simpleType.TypeName),
                                        simpleType.Annotation.IsNotNullOrEmpty() ? new XElement(N("annotation"), new XElement(N("documentation"), simpleType.Annotation)) : null,
                                        new XElement(N("restriction"), new XAttribute("base", simpleType.BaseTypeName),
                                            simpleType.MinLength > 0 ? new XElement(N("minLength"), new XAttribute("value", simpleType.MinLength)) : null,
                                            simpleType.Pattern.IsNotNullOrEmpty() ? new XElement(N("pattern"), new XAttribute("value", simpleType.Pattern)) : null,
                                            simpleType.Enumeration.IsNotNull() && simpleType.Enumeration.Count > 0 ? GetEnumerationXml(simpleType.Enumeration) : null                                            
                                        )
                                    );
                                    xsd.Add(simpleTypeElement);
                                }

                                var xsdElementItems = xsdElements.Where(x => x.Attribute("name").Value == propertyName);
                                foreach (var xsdElement in xsdElementItems) {
                                    if (xsdElement.Attribute("type").IsNotNull()) {
                                        xsdElement.Attribute("type").Value = simpleType.Prefix + ":" + simpleType.TypeName;
                                    }
                                    else {
                                        xsdElement.Add(new XAttribute("type", simpleType.Prefix + ":" + simpleType.TypeName));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return xsd;
        }

        private List<XElement> GetEnumerationXml(List<string> enumeration) {
            var list = new List<XElement>();
            if (enumeration.IsNotNull() && enumeration.Count > 0) {
                foreach (var item in enumeration) {
                    list.Add(new XElement(N("enumeration"), new XAttribute("value", item)));
                }
            }

            return list;
        }

        private XName N(string localName) { return XName.Get(localName, "http://www.w3.org/2001/XMLSchema"); }

        public void Dispose() { }

        private Type GetClassType(string xsdTypeName) {
            if (Types.IsNotNull() && xsdTypeName.IsNotNullOrEmpty()) {
                foreach (var type in Types) {
                    var xmlTypeName = GetXmlTypeName(type);
                    if (xmlTypeName.IsNotNullOrEmpty() && xmlTypeName.Equals(xsdTypeName)) {
                        return type;
                    }
                }
            }
            return default;
        }

        private string GetTypeAnnotation(Type type) {
            if (type.IsNotNull()) {
                XmlAnnotationAttribute[] attributes = (XmlAnnotationAttribute[])type.GetCustomAttributes(typeof(XmlAnnotationAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0].Annotation;
                }
            }
            return default;
        }

        private string GetPropertyAnnotation(PropertyInfo property) {
            if (property.IsNotNull()) {
                XmlAnnotationAttribute[] attributes = (XmlAnnotationAttribute[])property.GetCustomAttributes(typeof(XmlAnnotationAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0].Annotation;
                }
            }
            return default;
        }

        private XmlSimpleTypeAttribute GetPropertySimpleType(PropertyInfo property) {
            if (property.IsNotNull()) {
                XmlSimpleTypeAttribute[] attributes = (XmlSimpleTypeAttribute[])property.GetCustomAttributes(typeof(XmlSimpleTypeAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0];
                }
            }
            return default;
        }

        private string GetPropertyXmlName(PropertyInfo property) {
            if (property.IsNotNull()) {
                XmlElementAttribute[] attributes = (XmlElementAttribute[])property.GetCustomAttributes(typeof(XmlElementAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0].ElementName;
                }
                else {
                    XmlAttributeAttribute[] attributes2 = (XmlAttributeAttribute[])property.GetCustomAttributes(typeof(XmlAttributeAttribute), false);
                    if (attributes2.IsNotNull() && attributes2.Length > 0) {
                        return attributes2[0].AttributeName;
                    }
                }

                return property.Name;
            }
            return default;
        }

        private bool GetPropertyRequired(PropertyInfo property) {
            if (property.IsNotNull()) {
                XmlRequiredAttribute[] attributes = (XmlRequiredAttribute[])property.GetCustomAttributes(typeof(XmlRequiredAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0].Required;
                }
            }
            return default;
        }

        private string GetXmlTypeName(Type type) {
            if (type.IsNotNull()) {
                XmlTypeAttribute[] attributes = (XmlTypeAttribute[])type.GetCustomAttributes(typeof(XmlTypeAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0].TypeName;
                }
                else {
                    return type.Name;
                }
            }
            return default;
        }
    }
}
