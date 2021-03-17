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
using Abc.Nes.Validators;
using System;
using System.Collections.Generic;

namespace Abc.Nes.ArchivalPackage {
    public interface IPackageManager : IDisposable {
        Package Package { get; }
        string FilePath { get; }
        void AddFile(DocumentFile document, IDocument metadata = null);
        void AddFile(string filePath, IDocument metadata = null);
        void AddFiles(IEnumerable<DocumentFile> documents, string folderName = null, IEnumerable<IDocument> metadata = null);
        void AddFiles(IEnumerable<string> files, string folderName = null, IEnumerable<IDocument> metadata = null);
        void AddObject(IDocument metadata, string fileName);
        void Save(string filePath = null, bool appendFileDataOnly = false);
        void LoadPackage(string filePath);
        int GetDocumentsCount();
        IEnumerable<ItemBase> GetAllFiles();
        IEnumerable<ItemBase> GetAllFiles(FolderBase folder);
        IEnumerable<DocumentFile> GetAllDocuments();
        bool Validate(out string message, bool validateMetdataFiles = false, bool breakOnFirstError = true);
        IValidationResult GetValidationResult(bool validateMetdataFiles = false, bool breakOnFirstError = true);

        FolderBase GetParentFolder(ItemBase item);
        FolderBase GetParentFolder(string filePath);
        ItemBase GetItemByFilePath(string filePath);
        MetadataFile GetMetadataFile(ItemBase documentFile);
    }
}
