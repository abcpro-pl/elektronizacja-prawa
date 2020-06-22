/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Collections.Generic;

namespace Abc.Nes.ArchivalPackage.Model {
    public class DocumentFile {
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
    }

    public class DocumentSubFolder {
        public string FolderName { get; set; }
        public List<DocumentFile> Files { get; set; }
        public List<DocumentSubFolder> SubFolders { get; set; }
    }
}
