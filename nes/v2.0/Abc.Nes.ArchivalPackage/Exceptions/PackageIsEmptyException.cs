/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;

namespace Abc.Nes.ArchivalPackage.Exceptions {
    public class PackageIsEmptyException : Exception {
        public PackageIsEmptyException() : base("Package is empty! Please, add files, metadata and official matter (objects) to the archive.") { }
    }
}
