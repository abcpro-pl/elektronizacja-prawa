/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.ArchivalPackage.Model;
using System.Collections.Generic;

namespace Abc.Nes.ArchivalPackage {
    public interface IPackageManager {
		Package Package { get; }
		string FilePath { get; }
		void AddFile(DocumentFile document, Document metadata = null);
		void AddFile(string filePath, Document metadata = null);
		void AddFiles(IEnumerable<string> files, string folderName, IEnumerable<Document> metadata = null);
		void AddObject(Document metadata, string fileName);
		void Save(string filePath = null);
		void LoadPackage(string filePath);
		int GetDocumentsCount();
		IEnumerable<ItemBase> GetAllFiles();
		IEnumerable<ItemBase> GetAllFiles(FolderBase folder);
		bool Validate(out string message);

		FolderBase GetParentFolder(ItemBase item);
		FolderBase GetParentFolder(string filePath);
		ItemBase GetItemByFilePath(string filePath);
		MetadataFile GetMetadataFile(ItemBase documentFile);
	}
}
