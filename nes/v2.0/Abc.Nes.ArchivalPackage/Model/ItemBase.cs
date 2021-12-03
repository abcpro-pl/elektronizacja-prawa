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
    public abstract class ItemBase {
		public string FileName { get; set; }
		public string FilePath { get; internal set; }
		public abstract void Init(byte[] fileData);

		public string GetSubFolderName() {
			if (FilePath.IsNotNullOrEmpty()) {
				var table = FilePath.Split('/', '\\');
				if (table.IsNotNull() && table.Length > 2) {
					return table[table.Length - 2];
				}
			}
			return default;
		}
	}
}
