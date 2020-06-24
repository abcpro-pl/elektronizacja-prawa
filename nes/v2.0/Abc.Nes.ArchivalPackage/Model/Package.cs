/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


namespace Abc.Nes.ArchivalPackage.Model {
    public class Package {
        public DocumentFolder Documents { get; set; }
        public MetdataFolder Metadata { get; set; }
        public MetdataFolder Objects { get; set; }
        public bool IsEmpty {
            get {
                return (Documents.IsNull() || Documents.IsEmpty) || (Metadata.IsNull() || Metadata.IsEmpty) || (Objects.IsNull() || Objects.IsEmpty);
            }
        }
    }
}
