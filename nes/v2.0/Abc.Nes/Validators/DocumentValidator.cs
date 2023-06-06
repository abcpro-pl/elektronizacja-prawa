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
using System.Collections;
using System.Linq;
using System.Xml.Serialization;

namespace Abc.Nes.Validators {
    public class DocumentValidator : IDocumentValidator {
        private ValidationResult _result = null;
        private System.Resources.ResourceManager resx = null;
        private string _filePath = null;
        public void Dispose() { }

        public IValidationResult Validate(IDocument document, string filePath) {
            _filePath = filePath;
            _result = new ValidationResult() { FilePath = filePath };
            resx = Properties.Default.ResourceManager;
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "pl" || System.Globalization.CultureInfo.CurrentCulture.Name == "pl-PL") {
                resx = Properties.Polish.ResourceManager;
            }

            if (document.IsNull()) { throw new ArgumentException(); }
            ValidateObject(document);

            //foreach (var item in _result.Where(x=>x.FilePath.IsNull()) {
            //    item.FilePath = filePath;
            //}

            //check grouping for documents, not for cases
            if (!filePath.ToLower().StartsWith("sprawy")) {
                var propName = "grupowanie";

                bool groupCaseMissing = false;
                string description = default;
                System.Collections.Generic.List<Elements.GroupingElement> groupings = default;
                switch (document.DocumentType) {
                    case Enumerations.DocumentType.Nes16:
                        var doc16 = document as Document16;
                        groupings = doc16.Groupings;
                        description = doc16.Description;
                        break;
                    case Enumerations.DocumentType.Nes17:
                        var doc17 = document as Document17;
                        description = doc17.Description;
                        groupings = doc17.Groupings;
                        break;
                    case Enumerations.DocumentType.Nes20:
                        var doc20 = document as Document;
                        description = doc20.Description;
                        groupings = doc20.Groupings;
                        break;
                }

                if (groupings.IsNotNull()) {
                    if (groupings.Count == 0 || !groupings.Any(x => x.Type.ToLower() == "znak sprawy")) {
                        groupCaseMissing = true;
                    }
                }

                if (groupCaseMissing) {
                    _result.Add(new ValidationResultItem() {
                        Source = ValidationResultSource.Metadata,
                        Type = ValidationResultType.GroupingCase,
                        Name = filePath,
                        FullName = description,
                        DefaultMessage = string.Format(resx.GetString("RequiredItemNotFound"), $"{propName}/znak sprawy"),
                        FilePath = filePath
                    });
                }
            }

            //if(document.)

            return _result;
        }


        private void ValidateObject(object o, string parentPropertyName = null) {
            // the object, due to its complexity, is currently not validated
            if (o.GetType().GetCustomAttributes(typeof(XmlChoiceAttribute), false).FirstOrDefault().IsNotNull()) { return; }

            var properties = o.GetType().GetProperties();
            foreach (var property in properties) {
                var rqAttr = property.GetCustomAttributes(typeof(XmlRequiredAttribute), false).FirstOrDefault() as XmlRequiredAttribute;
                if (rqAttr.IsNotNull() && rqAttr.Required) {

                    // ------ Property Name ------------------
                    var propertyName = property.Name;
                    {
                        var xmlElementAttr = property.GetCustomAttributes(typeof(XmlElementAttribute), false).FirstOrDefault() as XmlElementAttribute;
                        if (xmlElementAttr.IsNotNull()) {
                            propertyName = xmlElementAttr.ElementName;
                        }
                        else {
                            var xmlRootAttr = property.GetCustomAttributes(typeof(XmlRootAttribute), false).FirstOrDefault() as XmlRootAttribute;
                            if (xmlRootAttr.IsNotNull()) {
                                propertyName = xmlRootAttr.ElementName;
                            }
                            else {
                                var xmlAttributeAttr = property.GetCustomAttributes(typeof(XmlAttributeAttribute), false).FirstOrDefault() as XmlAttributeAttribute;
                                if (xmlAttributeAttr.IsNotNull()) {
                                    propertyName = xmlAttributeAttr.AttributeName;
                                }
                            }
                        }
                    }
                    // ---------------------------------------

                    // -------- Object Name ------------------
                    var objectType = o.GetType();
                    var objectFullName = parentPropertyName.IsNotNullOrEmpty() ? parentPropertyName : objectType.FullName;
                    if (parentPropertyName.IsNullOrEmpty()) {
                        var xmlElementAttr = objectType.GetCustomAttributes(typeof(XmlElementAttribute), false).FirstOrDefault() as XmlElementAttribute;
                        if (xmlElementAttr.IsNotNull()) {
                            objectFullName = xmlElementAttr.ElementName;
                        }
                        else {
                            var xmlRootAttr = objectType.GetCustomAttributes(typeof(XmlRootAttribute), false).FirstOrDefault() as XmlRootAttribute;
                            if (xmlRootAttr.IsNotNull()) {
                                objectFullName = xmlRootAttr.ElementName;
                            }
                            else {
                                var xmlAttributeAttr = objectType.GetCustomAttributes(typeof(XmlAttributeAttribute), false).FirstOrDefault() as XmlAttributeAttribute;
                                if (xmlAttributeAttr.IsNotNull()) {
                                    objectFullName = xmlAttributeAttr.AttributeName;
                                }
                            }
                        }
                    }
                    // ---------------------------------------

                    var propVal = property.GetValue(o);
                    if (propVal.IsNull()) {
                        _result.Add(new ValidationResultItem() {
                            Source = ValidationResultSource.Metadata,
                            Type = ValidationResultType.DoesNotHaveValue,
                            Name = propertyName.IsNotNullOrEmpty() ? propertyName : property.Name,
                            FullName = o.GetType().FullName,
                            FilePath = _filePath,
                            DefaultMessage = String.Format(resx.GetString("RequiredFieldHasNoValue"), propertyName, objectFullName)
                        });
                        continue;
                    }
                    if (propVal is ICollection) {
                        if ((propVal as ICollection).Count == 0) {
                            _result.Add(new ValidationResultItem() {
                                Source = ValidationResultSource.Metadata,
                                Type = ValidationResultType.HasNoElements,
                                Name = propertyName.IsNotNullOrEmpty() ? propertyName : property.Name,
                                FullName = o.GetType().FullName,
                                FilePath = _filePath,
                                DefaultMessage = String.Format(resx.GetString("RequiredFieldHasNoItems"), propertyName, objectFullName)
                            });
                        }
                        else {
                            var collection = propVal as ICollection;
                            foreach (var item in collection) {
                                ValidateObject(item, propertyName);
                            }
                        }
                    }
                    else {
                        ValidateObject(propVal, propertyName);
                    }
                }
            }

        }
    }
}
