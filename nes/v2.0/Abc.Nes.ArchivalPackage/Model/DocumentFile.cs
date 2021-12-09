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
using System.Collections.Generic;
using System.Linq;

namespace Abc.Nes.ArchivalPackage.Model {
    public class DocumentFile : ItemBase {
        public byte[] FileData { get; set; }        
        public override void Init(byte[] fileData, out Exception ex) {
            ex = null;
            FileData = fileData;
        }
    }

    public class DocumentFolder : Folder<DocumentFolder, DocumentFile> {
        public override List<DocumentFile> Items { get; set; }
        public override List<DocumentFolder> Folders { get; set; }
        public override FolderBase CreateSubFolder(string folderName) {
            if (folderName.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            if (Folders.IsNotNull()) {
                var _folder = Folders.Where(x => x.FolderName == folderName).FirstOrDefault();
                if (_folder.IsNotNull()) {
                    return _folder;
                }
            }
            var folder = new DocumentFolder() {
                FolderName = folderName,
                Folders = new List<DocumentFolder>(),
                Items = new List<DocumentFile>()
            };
            if (Folders.IsNull()) { Folders = new List<DocumentFolder>(); }
            Folders.Add(folder);
            return folder;
        }
    }
}
