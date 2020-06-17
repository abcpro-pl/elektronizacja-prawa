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
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "element-bazowy-typ")]
    [XmlAnnotation("Element bazowy.")]
    public abstract class ElementBase : IComparable, IDisposable {
        public int CompareTo(object obj) {
            if (obj.IsNull()) { return 1; }
            return this.GetHashCode().CompareTo(obj.GetHashCode());
        }

        public void Dispose() { }
    }
}
