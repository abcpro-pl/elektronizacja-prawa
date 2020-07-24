/*=====================================================================================

	ABC NES.ArchivalPackage.Cryptography 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public class PackageSignerManager : IDisposable {
        public void Dispose() { }
    }
}
