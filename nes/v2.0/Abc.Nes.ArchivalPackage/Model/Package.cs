﻿/*=====================================================================================

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
        public DocumentSubFolder Documents { get; set; }
        public MetdataSubFolder Metdata { get; set; }
        public MetdataSubFolder Objects { get; set; }
    }
}
