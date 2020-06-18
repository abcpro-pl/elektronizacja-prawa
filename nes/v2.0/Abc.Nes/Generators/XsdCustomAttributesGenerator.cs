/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Abc.Nes.Generators {
    public class XsdCustomAttributesGenerator : IDisposable {
        readonly List<Type> Types = null;
        private XsdCustomAttributesGenerator() { }
        public XsdCustomAttributesGenerator(params string[] namespaces) : this() {
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
        public XElement AddCustomAttributes(XElement xsd, bool addCustomAttributes = true) {
            if (xsd.IsNotNull() && xsd.HasElements) {

                // Add annotations for classes
                var xsdTypes = xsd.Elements().Where(x => (x.Name.LocalName == "complexType" || x.Name.LocalName == "simpleType") && x.Attribute("name").IsNotNull() && x.Attribute("name").Value.IsNotNullOrEmpty());
                foreach (var xsdType in xsdTypes) {
                    var typeName = xsdType.Attribute("name").Value;
                    var type = GetClassType(typeName);
                    var annotation = GetTypeAnnotation(type);
                    if (annotation.IsNotNullOrEmpty() && xsdType.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                        xsdType.AddFirst(new XElement(N("annotation"), new XElement(N("documentation"), new XText(annotation))));

                        var elements = xsd.Descendants().Where(x => x.Attribute("type").IsNotNull() && (x.Attribute("type").Value == typeName || x.Attribute("type").Value.EndsWith($":{typeName}")));
                        foreach (var element in elements) {
                            if (element.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                                element.AddFirst(new XElement(N("annotation"), new XElement(N("documentation"), new XText(annotation))));
                            }
                        }
                    }
                }

                // Add annotations for properties
                foreach (var xsdType in xsdTypes) {
                    var typeName = xsdType.Attribute("name").Value;
                    var type = GetClassType(typeName);

                    if (addCustomAttributes && type.IsNotNull()) {
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
                                            xsdElement.AddFirst(new XElement(N("annotation"), new XElement(N("documentation"), new XText(propertyAnnotation))));
                                        }


                                        if (xsdElement.Attribute("type").IsNotNull()) {
                                            var xsdElementType = xsdElement.Attribute("type").Value;
                                            if (xsdElementType.Contains(":")) { xsdElementType = xsdElementType.Substring(xsdElementType.IndexOf(":") + 1); }

                                            var _elementType = xsdTypes.Where(x => x.Attribute("name").Value == xsdElementType).FirstOrDefault();
                                            if (_elementType.IsNotNull() && _elementType.Elements().Where(x => x.Name.LocalName == "annotation").Count() == 0) {
                                                _elementType.AddFirst(new XElement(N("annotation"), new XElement(N("documentation"), new XText(propertyAnnotation))));
                                            }
                                        }
                                    }
                                }
                            }

                            // add requrired attributes
                            var required = GetPropertyRequired(property);
                            if (required == BooleanValues.True) {
                                var xsdItems = xsdElements.Where(x => x.Attribute("name").Value == propertyName);
                                if (xsdItems.IsNotNull() && xsdItems.Count() > 0 && xsdItems.First().Parent.Name.LocalName == "choice") {
                                    xsdItems = xsdItems.First().Parent.Elements();
                                }
                                foreach (var item in xsdItems) {
                                    if (item.Name.LocalName == "attribute") {
                                        if (item.Attribute("use").IsNotNull()) {
                                            item.Attribute("use").Value = "required";
                                        }
                                        else {
                                            item.Add(new XAttribute("use", "required"));
                                        }
                                    }
                                    else {
                                        if (item.Attribute("minOccurs").IsNotNull()) {
                                            item.Attribute("minOccurs").Remove();
                                        }
                                    }
                                }
                            }
                            else if (required == BooleanValues.False) {
                                var xsdItems = xsdElements.Where(x => x.Attribute("name").Value == propertyName);
                                foreach (var item in xsdItems) {
                                    if (item.Name.LocalName == "attribute") {
                                        if (item.Attribute("use").IsNotNull() && item.Attribute("use").Value == "required") {
                                            item.Attribute("use").Value = "optional";
                                        }
                                    }
                                    else {
                                        if (item.Attribute("minOccurs").IsNotNull() && item.Attribute("minOccurs").Value != "0") {
                                            item.Attribute("minOccurs").Value = "0";
                                        }
                                        else {
                                            item.Add(new XAttribute("minOccurs", "0"));
                                        }
                                    }
                                }
                            }

                            // add simple types 
                            var simpleType = GetPropertySimpleType(property);
                            if (simpleType.IsNotNull()) {

                                var simpleTypeElement = xsd.Elements().Where(x => x.Name.LocalName == "simpleType" && x.Attribute("name").Value == simpleType.TypeName).FirstOrDefault();
                                if (simpleTypeElement.IsNull()) {

                                    if (simpleType.UnionMemberTypes.IsNotNullOrEmpty()) {
                                        simpleTypeElement = new XElement(N("simpleType"),
                                                new XAttribute("name", simpleType.TypeName),
                                                simpleType.Annotation.IsNotNullOrEmpty() ? new XElement(N("annotation"), new XElement(N("documentation"), simpleType.Annotation)) : null,
                                                (simpleType.EnumerationRestriction.IsNull() || simpleType.EnumerationRestriction.Count() == 0) && simpleType.MinLength > 0 ? new XElement(N("minLength"), new XAttribute("value", simpleType.MinLength)) : null,
                                                simpleType.Pattern.IsNotNullOrEmpty() ? new XElement(N("pattern"), new XAttribute("value", simpleType.Pattern)) : null,
                                                new XElement(N("union"), new XAttribute("memberTypes", simpleType.UnionMemberTypes),
                                                    simpleType.EnumerationRestriction.IsNotNull() && simpleType.EnumerationRestriction.Length > 0 ? new XElement(N("simpleType"), GetEnumerationRestrictionXml(simpleType.EnumerationRestriction, simpleType.BaseTypeName)) : null
                                                ));
                                    }
                                    else {
                                        simpleTypeElement = new XElement(N("simpleType"),
                                            new XAttribute("name", simpleType.TypeName),
                                            simpleType.Annotation.IsNotNullOrEmpty() ? new XElement(N("annotation"), new XElement(N("documentation"), simpleType.Annotation)) : null,
                                            new XElement(N("restriction"), new XAttribute("base", simpleType.BaseTypeName),
                                                simpleType.MinLength > 0 ? new XElement(N("minLength"), new XAttribute("value", simpleType.MinLength)) : null,
                                                simpleType.Pattern.IsNotNullOrEmpty() ? new XElement(N("pattern"), new XAttribute("value", simpleType.Pattern)) : null,
                                                simpleType.EnumerationRestriction.IsNotNull() && simpleType.EnumerationRestriction.Length > 0 ? GetEnumerationRestrictionXml(simpleType.EnumerationRestriction, simpleType.BaseTypeName) : null
                                            )
                                        );
                                    }
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

                // create choices and groups
                var choiceClasses = GetChoiceClasses();
                if (choiceClasses.IsNotNull() && choiceClasses.Count() > 0) {
                    foreach (var cClass in choiceClasses) {
                        var typeName = GetXmlTypeName(cClass);
                        var groups = GetGroups(cClass);
                        var xChoice = new XElement(N("choice"));
                        if (groups.IsNotNull() && groups.Count() > 0) {
                            foreach (var g in groups) {
                                xChoice.Add(new XElement(N("group"), new XAttribute("ref", $"{g.Prefix}:{g.Name}")));
                            }
                        }

                        var xsdElement = xsd.Elements(N("complexType")).Where(x => x.Attribute("name").IsNotNull() && x.Attribute("name").Value == typeName).FirstOrDefault();

                        if (xChoice.HasElements && xsdElement.IsNotNull()) {
                            var xsdSequence = xsdElement.Element(N("sequence"));
                            if (xsdSequence.IsNotNull()) {
                                // move elements to groups
                                if (groups.IsNotNull() && groups.Count() > 0) {
                                    foreach (var g in groups) {
                                        var pNames = GetGroupProperties(cClass, g.Name);
                                        var groupXml = new XElement(N("group"), new XAttribute("name", g.Name));
                                        if (g.Annotation.IsNotNullOrEmpty()) {
                                            groupXml.Add(new XElement(N("annotation"), new XElement(N("documentation"), new XText(g.Annotation))));
                                        }
                                        var groupSeq = new XElement(N("sequence"));
                                        groupXml.Add(groupSeq);
                                        foreach (var pName in pNames) {
                                            groupSeq.Add(xsdSequence.Elements().Where(x => x.Attribute("name").IsNotNull() && x.Attribute("name").Value == pName));
                                        }
                                        xsd.Add(groupXml);
                                    }
                                }

                                // replace content with choice
                                xsdSequence.Remove();
                                xsdElement.Add(xChoice);
                            }
                        }
                    }
                }
            }
            return xsd;
        }

        private XElement GetEnumerationRestrictionXml(IEnumerable<string> enumeration, string baseType) {
            var restriction = new XElement(N("restriction"), new XAttribute("base", baseType));
            if (enumeration.IsNotNull() && enumeration.Count() > 0) {
                foreach (var item in enumeration) {
                    restriction.Add(new XElement(N("enumeration"), new XAttribute("value", item)));
                }
            }

            return restriction;
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

        private IEnumerable<string> GetGroupProperties(Type type, string groupName) {
            if (type.IsNotNull()) {
                var list = new List<string>();
                var properties = type.GetProperties();
                foreach (var property in properties) {
                    var g = property.GetCustomAttribute(typeof(XmlGroupAttribute)) as XmlGroupAttribute;
                    if (g.IsNotNull() && g.Name == groupName) {
                        list.Add(GetPropertyXmlName(property));
                    }
                }
                return list;
            }
            return default;
        }
        private IEnumerable<XmlGroupAttribute> GetGroups(Type type) {
            if (type.IsNotNull()) {
                var list = new List<XmlGroupAttribute>();
                var properties = type.GetProperties();
                foreach (var property in properties) {
                    var g = property.GetCustomAttribute(typeof(XmlGroupAttribute)) as XmlGroupAttribute;
                    if (g.IsNotNull() && list.Where(x => x.Name == g.Name).Count() == 0) {
                        list.Add(g);
                    }
                }
                return list;
            }
            return default;
        }
        private IEnumerable<Type> GetChoiceClasses() {
            if (Types.IsNotNull()) {
                var list = new List<Type>();
                foreach (var type in Types) {
                    if (type.GetCustomAttributes(typeof(XmlChoiceAttribute), false).Length > 0) {
                        list.Add(type);
                    }
                }
                return list;
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

        private BooleanValues GetPropertyRequired(PropertyInfo property) {
            if (property.IsNotNull()) {
                XmlRequiredAttribute[] attributes = (XmlRequiredAttribute[])property.GetCustomAttributes(typeof(XmlRequiredAttribute), false);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    return attributes[0].Required ? BooleanValues.True : BooleanValues.False;
                }
            }
            return BooleanValues.None;
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
