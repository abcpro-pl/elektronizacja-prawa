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
using System.Linq;
using System.Xml.Linq;

namespace Abc.Nes.Converters {
    class XmlUpdater {
        private XElement Xml = null;

        private XName N(string localName) { return XName.Get(localName, "http://www.mswia.gov.pl/standardy/ndap"); }

        public string UpdateXmlText(string xmlText) {
            if (xmlText.IsNotNullOrEmpty()) {
                try {
                    Xml = XElement.Parse(xmlText);

                    UpdateIds();
                    UpdateDates();
                    UpdateTitles();
                    UpdateLanguages();
                    UpdateFormats();
                    UpdateTypes();
                    UpdateAddresses();
                    UpdateInstitutions();
                    UpdatePeople();
                    UpdateAccess();
                    UpdateRelations();
                    UpdateGrouping();

                    xmlText = Xml.ToString();
                }
                catch { }
            }
            return xmlText;
        }

        private void UpdateGrouping() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "grupowanie");
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "typ").ToList().ForEach(x => {
                    x.Name = N("typGrupy");
                });
                item.Elements().Where(x => x.Name.LocalName == "kod").ToList().ForEach(x => {
                    x.Name = N("kopGrupy");
                });
            }
        }

        private void UpdateRelations() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "relacja");
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "typ").ToList().ForEach(x => {
                    x.Name = N("typRelacji");
                });
            }
        }

        private void UpdateTypes() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "typ");
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "kategoria").ToList().ForEach(x => {
                    x.Name = N("klasa");
                    if (x.Value.ToLower() == "collection") { x.Value = DocumentClassType.Collection.GetXmlEnum(); }
                    if (x.Value.ToLower() == "dataset") { x.Value = DocumentClassType.DataSet.GetXmlEnum(); }
                });
            }
        }

        private void UpdateFormats() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "format");
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "typ").ToList().ForEach(x => {
                    x.Name = N("typFormatu");
                });
            }
        }

        private void UpdateTitles() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "tytul");
            foreach (var item in items) {
                foreach (var title in item.Elements()) {
                    var attr = title.Attributes().Where(x => x.Name.LocalName == "jezyk").FirstOrDefault();
                    if (attr.IsNotNull()) {
                        var attrVal = attr.Value;
                        if (attrVal == "pl" || attrVal == "pl-PL" || attrVal == "polski") { attrVal = "pol"; }
                        attr.Remove();
                        title.Add(new XAttribute("kodJezyka", attrVal));
                    }
                }
            }
        }

        private void UpdateLanguages() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "jezyk");
            foreach (var item in items) {
                var attr = item.Attributes().Where(a => a.Name.LocalName == "nazwa" || a.Name.LocalName == "Name" || a.Name.LocalName == "kod").FirstOrDefault();
                if (attr.IsNotNull()) {
                    var attrVal = attr.Value;
                    if (attrVal == "pl" || attrVal == "pl-PL" || attrVal == "polski") { attrVal = "pol"; }
                    attr.Remove();
                    item.Add(new XAttribute("kodJezyka", attrVal));
                }
            }
        }

        private void UpdateAddresses() {
            var items = Xml.Descendants().Where(x => x.Name.LocalName == "adres" && x.HasElements);
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "kod").ToList().ForEach(x => {
                    x.Name = N("kodPocztowy");
                });

                item.Elements().Where(x => x.Name.LocalName == "skrytkapocztowa").ToList().ForEach(x => {
                    x.Name = N("skrytkaPocztowa");
                });

                item.Elements().Where(x => x.Name.LocalName == "kraj").ToList().ForEach(x => {
                    if (x.Value.ToLower() == "polska") {
                        x.Value = CountryCodeType.PL.GetXmlEnum();
                    }
                });
            }
        }

        private void UpdateInstitutions() {
            var items = Xml.Descendants().Where(x => x.Name.LocalName == "instytucja" && x.HasElements);
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "id").ToList().ForEach(x => {
                    var attr = x.Attributes().Where(a => a.Name.LocalName == "typ").FirstOrDefault();
                    if (attr.IsNotNull()) {
                        var attrVal = attr.Value;
                        attr.Remove();
                        x.Add(new XAttribute("typId", attrVal));
                    }
                });
            }
        }

        private void UpdatePeople() {
            var items = Xml.Descendants().Where(x => x.Name.LocalName == "osoba" && x.HasElements);
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "id").ToList().ForEach(x => {
                    var attr = x.Attributes().Where(a => a.Name.LocalName == "typ").FirstOrDefault();
                    if (attr.IsNotNull()) {
                        var attrVal = attr.Value;
                        attr.Remove();
                        x.Add(new XAttribute("typId", attrVal));
                    }
                });
            }
        }

        private void UpdateIds() {
            var items = Xml.Descendants().Where(x => x.Name.LocalName == "identyfikator" && x.HasElements);
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "typ").ToList().ForEach(x => {
                    x.Name = N("typidentyfikatora");
                });

                item.Elements().Where(x => x.Name.LocalName == "wartosc").ToList().ForEach(x => {
                    x.Name = N("wartoscId");
                });
            }
        }

        private void UpdateDates() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "data");
            foreach (var item in items) {
                item.Elements().Where(x => x.Name.LocalName == "typ").ToList().ForEach(x => {
                    x.Name = N("typDaty");
                    if (x.Value == "stworzony") { x.Value = EventDateType.Created.GetXmlEnum(); }
                    if (x.Value == "dostepnyPo") { x.Value = EventDateType.AvailableAfter.GetXmlEnum(); }
                    if (x.Value == "wyslany") { x.Value = EventDateType.Sent.GetXmlEnum(); }
                });
            }
        }

        public void UpdateAccess() {
            var items = Xml.Elements().Where(x => x.Name.LocalName == "dostep");
            foreach (var item in items) {
                var date = item.Element(N("data"));
                if (date.IsNotNull()) {
                    var type = date.Element(N("typ"));
                    if (type.IsNotNull()) {
                        if (type.Value != AccessDateType.After.GetXmlEnum()) {
                            type.Value = AccessDateType.After.GetXmlEnum();
                        }
                    }
                }
            }
        }
    }
}
