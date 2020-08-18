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
        private ValidationResult Result = null;

        public void Dispose() { }

        public IValidationResult Validate(Document document) {
            Result = new ValidationResult();

            if (document.IsNull()) { throw new ArgumentException(); }
            ValidateObject(document);

            return Result;
        }


        private void ValidateObject(object o) {
            if (o.GetType().GetCustomAttributes(typeof(XmlChoiceAttribute), false).FirstOrDefault().IsNotNull()) { return; }

            var properties = o.GetType().GetProperties();
            foreach (var property in properties) {
                var rqAttr = property.GetCustomAttributes(typeof(XmlRequiredAttribute), false).FirstOrDefault() as XmlRequiredAttribute;
                if (rqAttr.IsNotNull() && rqAttr.Required) {

                    var propVal = property.GetValue(o);
                    if (propVal.IsNull()) {
                        Result.Add(new ValidationResultItem() {
                            Source = ValidationResultSource.Metadata,
                            Type = ValidationResultType.DoesNotHaveValue,
                            Name = property.Name,
                            FullName = o.GetType().FullName,
                            DefaultMessage = $"Required field '{property.Name}' of type '{o.GetType().FullName}' does not have a value!"
                        });
                        continue;
                    }
                    if (propVal is ICollection) {
                        if ((propVal as ICollection).Count == 0) {
                            Result.Add(new ValidationResultItem() {
                                Source = ValidationResultSource.Metadata,
                                Type = ValidationResultType.HasNoElements,
                                Name = property.Name,
                                FullName = o.GetType().FullName,
                                DefaultMessage = $"Required list of fields '{property.Name}' of type '{o.GetType().FullName}' has no elements!"
                            });
                        }
                        else {
                            var collection = propVal as ICollection;
                            foreach (var item in collection) {
                                ValidateObject(item);
                            }
                        }
                    }
                    else {
                        ValidateObject(propVal); // rekurencyjnie
                    }
                }
            }

        }
    }
}
