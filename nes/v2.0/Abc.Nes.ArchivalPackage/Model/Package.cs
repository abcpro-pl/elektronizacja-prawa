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
using System.Linq;
using System.Text.RegularExpressions;

namespace Abc.Nes.ArchivalPackage.Model {
    public class Package {
        public string FilePath { get; }
        public DocumentFolder Documents { get; set; }
        public MetdataFolder Metadata { get; set; }
        public MetdataFolder Objects { get; set; }
        public bool IsEmpty {
            get {
                return (Documents.IsNull() || Documents.IsEmpty) || (Metadata.IsNull() || Metadata.IsEmpty) || (Objects.IsNull() || Objects.IsEmpty);
            }
        }
        public Package() { }
        internal Package(string filePath) : this() {
            FilePath = filePath;
        }

        public IEnumerable<ItemBase> GetAllFiles() {
            var items = new List<ItemBase>();

            items.AddRange(GetAllFiles(Documents));
            items.AddRange(GetAllFiles(Metadata));
            items.AddRange(GetAllFiles(Objects));

            return items;
        }

        public IEnumerable<ItemBase> GetAllFiles(FolderBase folder) {
            var items = new List<ItemBase>();
            if (folder.IsNotNull()) {
                var folderItems = folder.GetItems();
                if (folderItems.IsNotNull() && folderItems.Count() > 0) items.AddRange(folderItems);

                var subfolders = folder.GetFolders();
                if (subfolders.IsNotNull() && folderItems.Count() > 0) {
                    foreach (var subfolder in subfolders) {
                        var subfolderItems = GetAllFiles(subfolder);
                        if (subfolderItems.IsNotNull() && subfolderItems.Count() > 0) items.AddRange(subfolderItems);
                    }
                }
            }
            return items;
        }

        public MetadataFile GetMetadataFile(ItemBase documentFile) {
            if (documentFile.IsNotNull() && documentFile.FilePath.IsNotNullOrEmpty()) {
                var regex = new Regex(Regex.Escape($"{MainDirectoriesName.Files.GetXmlEnum()}"));
                var metadataFilePath = regex.Replace(documentFile.FilePath, MainDirectoriesName.Metadata.GetXmlEnum(), 1);
                return GetItemByFilePath($"{metadataFilePath}.xml") as MetadataFile;
            }
            return default;
        }
        public ItemBase GetItemByFilePath(string filePath) {
            if (filePath.IsNotNullOrEmpty()) {
                FolderBase folder = null;
                var table = filePath.Split('/');

                for (int i = 0; i < table.Length; i++) {
                    var itemName = table[i].ToLower();

                    if (i == 0) {
                        if (itemName == Documents.FolderName.ToLower()) {
                            folder = Documents;
                            continue;
                        }
                        else if (itemName == Metadata.FolderName.ToLower()) {
                            folder = Metadata;
                            continue;
                        }
                        else if (itemName == Objects.FolderName.ToLower()) {
                            folder = Objects;
                            continue;
                        }
                    }

                    var folders = folder.GetFolders();
                    if (folders.IsNotNull() && folders.Count() > 0) {
                        var _folder = folders.Where(x => x.FolderName.ToLower() == itemName).FirstOrDefault();
                        if (_folder.IsNotNull()) {
                            folder = _folder;
                        }
                    }
                }

                if (folder.IsNotNull()) {
                    return folder.GetItems().Where(x => x.FileName.ToLower() == table.Last().ToLower()).FirstOrDefault();
                }
            }
            return default;
        }
    }
}
