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
    public class WrongMetadataCountException : Exception {
        public WrongMetadataCountException():base("Wrong metdata count! The number of files and metadata must be the same.") {}
    }
}
