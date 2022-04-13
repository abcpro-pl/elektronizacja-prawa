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
        public MetadataFolder Metadata { get; set; }
        public MetadataFolder Objects { get; set; }
        public DocumentFolder Another { get; set; }
        public bool IsEmpty => (Documents.IsNull() || Documents.IsEmpty) || (Metadata.IsNull() || Metadata.IsEmpty) || (Objects.IsNull() || Objects.IsEmpty);
        public bool HasAnotherFilesOrDirectories => Another.IsNotNull() && ((Another.Items.IsNotNull() && Another.Items.Count > 0) || (Another.Folders.IsNotNull() && Another.Folders.Count > 0));
        public Package() { }
        internal Package(string filePath) : this() {
            FilePath = filePath;
        }

        public IEnumerable<ItemBase> GetAllFiles() {
            var items = new List<ItemBase>();

            items.AddRange(GetAllFiles(Documents));
            items.AddRange(GetAllFiles(Metadata));
            items.AddRange(GetAllFiles(Objects));

            items.AddRange(GetAllFiles(Another));

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
        public MetadataFile GetMetadataFile(ItemBase documentFile, bool findWithoutSpaces = false) {
            if (documentFile.IsNotNull() && documentFile.FilePath.IsNotNullOrEmpty()) {
                var documentFilePath = documentFile.FilePath;
                string metadataFilePath;
                var regex = new Regex(Regex.Escape($"{MainDirectoriesName.Files.GetXmlEnum()}"));

                var subFolder = documentFile.GetSubFolderName();
                if (subFolder.IsNotNullOrEmpty()) {
                    // find metadata file for subfolder
                    metadataFilePath = documentFilePath.Substring(0, documentFilePath.LastIndexOf('/'));
                    metadataFilePath = regex.Replace(metadataFilePath, MainDirectoriesName.Metadata.GetXmlEnum(), 1);
                    var result = GetItemByFilePath($"{metadataFilePath}.xml", findWithoutSpaces) as MetadataFile;
                    if (result.IsNotNull()) {
                        return result;
                    }
                }

                // find metadata file for document file               
                {
                    metadataFilePath = regex.Replace(documentFilePath, MainDirectoriesName.Metadata.GetXmlEnum(), 1);
                    var result = GetItemByFilePath($"{metadataFilePath}.xml", findWithoutSpaces) as MetadataFile;
                    if (result.IsNotNull()) {
                        return result;
                    }
                }
            }
            return default;
        }
        public ItemBase GetItemByFilePath(string filePath, bool findWithoutSpaces = false) {
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
                        else if (itemName == Another.FolderName.ToLower()) {
                            folder = Another;
                            continue;
                        }
                        else {
                            // file in pseudo-another folder
                            folder = Another.GetFolder(itemName, true);
                            if (folder.IsNotNull()) { continue; }
                        }
                    }

                    var _folder = folder.GetFolder(itemName, true);
                    if (_folder.IsNotNull()) {
                        folder = _folder;
                    }
                }

                if (folder.IsNotNull()) {
                    ItemBase result = null;
                    var fileName = table.Last().ToLower();
                    if (findWithoutSpaces) {
                        fileName = fileName.FileGetFileNameWithoutExtension();
                        result = folder.GetItems().Where(x => x.FileName.FileGetFileNameWithoutExtension().CompareToWithoutSpaceAndCharCase(fileName)).FirstOrDefault();
                        if (result.IsNull() && fileName.Contains(".")) {
                            fileName = fileName.FileGetFileNameWithoutExtension();
                            result = folder.GetItems().Where(x => x.FileName.FileGetFileNameWithoutExtension().CompareToWithoutSpaceAndCharCase(fileName)).FirstOrDefault();
                        }
                    }
                    else {
                        result = folder.GetItems().Where(x => x.FileName.ToLower() == fileName).FirstOrDefault();
                    }

                    return result;
                }
            }
            return default;
        }
    }
}
