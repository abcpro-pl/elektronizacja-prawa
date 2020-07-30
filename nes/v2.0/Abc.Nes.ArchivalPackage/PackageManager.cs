/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.ArchivalPackage.Exceptions;
using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Converters;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Abc.Nes.ArchivalPackage {
    public class PackageManager : IPackageManager {
        public Package Package { get; private set; }
        public string FilePath { get; private set; }
        public void AddFile(DocumentFile document, Document metadata = null) {
            if (document.IsNull()) { throw new ArgumentNullException(); }
            if (Package.IsNull()) { InitializePackage(); }
            Package.Documents.Items.Add(document);
            if (metadata.IsNotNull()) {
                Package.Metadata.Items.Add(new MetadataFile() {
                    Document = metadata,
                    FileName = $"{document.FileName}.xml"
                });
            }
        }
        public void AddFile(string filePath, Document metadata = null) {
            if (filePath.IsNull()) { throw new ArgumentNullException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            if (Package.IsNull()) { InitializePackage(); }

            Package.Documents.Items.Add(new DocumentFile() {
                FileName = GetValidFileName(filePath),
                FileData = File.ReadAllBytes(filePath)
            });

            if (metadata.IsNotNull()) {
                Package.Metadata.Items.Add(new MetadataFile() {
                    Document = metadata,
                    FileName = $"{GetValidFileName(filePath)}.xml"
                });
            }
        }
        public void AddFiles(IEnumerable<string> files, string folderName, IEnumerable<Document> metadata = null) {
            if (files.IsNull()) { throw new NullReferenceException(); }
            if (folderName.IsNotNullOrEmpty()) { throw new NullReferenceException(); }

            if (files.Count() < 2) { throw new WrongFilesCountException(); }

            foreach (var filePath in files) {
                if (filePath.IsNull()) { throw new NullReferenceException(); }
                if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
            }

            if (metadata.IsNotNull() && metadata.Count() != files.Count()) {
                throw new WrongMetadataCountException();
            }

            if (Package.IsNull()) { InitializePackage(); }
            var folder = new DocumentFolder() {
                FolderName = folderName,
                Items = new List<DocumentFile>()
            };

            MetdataFolder metadataFolder = null;
            if (metadata.IsNotNull()) {
                metadataFolder = new MetdataFolder() {
                    FolderName = $"{folderName}.xml",
                    Items = new List<MetadataFile>()
                };
                Package.Metadata.Folders.Add(metadataFolder);
            }

            for (var i = 0; i < files.Count(); i++) {
                var filePath = files.ToArray()[i];
                folder.Items.Add(new DocumentFile() {
                    FileName = GetValidFileName(filePath),
                    FileData = File.ReadAllBytes(filePath)
                });

                if (metadata.IsNotNull()) {
                    var fileMetadata = metadata.ToArray()[i];
                    metadataFolder.Items.Add(new MetadataFile() {
                        Document = fileMetadata,
                        FileName = $"{GetValidFileName(filePath)}.xml"
                    });
                }
            }

            Package.Documents.Folders.Add(folder);
        }
        public void AddObject(Document metadata, string fileName) {
            if (metadata.IsNull()) { throw new ArgumentNullException("metadata"); }
            if (fileName.IsNullOrEmpty()) { throw new ArgumentNullException("fileName"); }
            if (!fileName.ToLower().EndsWith(".xml")) { throw new Exception("The Object file must by an XML file!"); }

            if (Package.IsNull()) { InitializePackage(); }

            Package.Objects.AddItem(new MetadataFile() {
                Document = metadata,
                FileName = fileName
            });
        }
        public void Save(string filePath = null) {
            if (Package.IsNull()) { throw new NullReferenceException("Package is not initialized!"); }
            if (Package.IsEmpty) { throw new PackageIsEmptyException(); }
            if (filePath.IsNullOrEmpty() && FilePath.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            if (filePath.IsNotNullOrEmpty()) { FilePath = filePath; }

            var tmp = Path.Combine(Path.GetTempPath(), $"{string.Empty.GenerateId()}.zip");
            var zipFile = new ZipFile(tmp);

            AddDocuments(Package.Documents, zipFile);
            AddMetadata(Package.Metadata, zipFile);
            AddMetadata(Package.Objects, zipFile);

            zipFile.Save();

            if (File.Exists(tmp)) {
                File.Copy(tmp, FilePath, true);
            }
        }
        public void LoadPackage(string filePath) {
            if (filePath.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(filePath); }
            if (!ZipFile.IsZipFile(filePath)) { throw new ZipException("Specified file is not a zip file!"); }

            FilePath = filePath;

            var zipFile = ZipFile.Read(filePath);

            IsPackageValid(zipFile);

            InitializePackage();

            foreach (var entry in zipFile.Entries) {
                if (entry.Attributes == FileAttributes.Directory) {
                    var dirName = entry.FileName;
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;

                    if (dirs[0] == MainDirectoriesName.Files.GetXmlEnum()) { folder = Package.Documents; }
                    if (dirs[0] == MainDirectoriesName.Metadata.GetXmlEnum()) { folder = Package.Metadata; }
                    if (dirs[0] == MainDirectoriesName.Objects.GetXmlEnum()) { folder = Package.Objects; }

                    foreach (var dir in dirs) {
                        if (dir == MainDirectoriesName.Files.GetXmlEnum() ||
                            dir == MainDirectoriesName.Metadata.GetXmlEnum() ||
                            dir == MainDirectoriesName.Objects.GetXmlEnum()) { continue; }

                        folder = folder.CreateSubFolder(dir);
                    }
                }
                else {
                    var dirName = Path.GetDirectoryName(entry.FileName).Replace("\\", "/");
                    var fileName = Path.GetFileName(entry.FileName);
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;
                    if (dirs[0] == MainDirectoriesName.Files.GetXmlEnum()) { folder = Package.Documents; }
                    if (dirs[0] == MainDirectoriesName.Metadata.GetXmlEnum()) { folder = Package.Metadata; }
                    if (dirs[0] == MainDirectoriesName.Objects.GetXmlEnum()) { folder = Package.Objects; }
                    foreach (var dir in dirs) {
                        if (dirs[0] == dir) { continue; }
                        var _folder = folder.GetFolder(dir);
                        if (_folder.IsNull()) { _folder = folder.CreateSubFolder(dir); }
                        folder = _folder;
                    }

                    var stream = new MemoryStream();
                    entry.Extract(stream);
                    var fileData = stream.ToArray();

                    ItemBase item;
                    if (dirs[0] == MainDirectoriesName.Files.GetXmlEnum()) {
                        item = new DocumentFile() { FileName = fileName, FilePath = entry.FileName };
                    }
                    else {
                        item = new MetadataFile() { FileName = fileName, FilePath = entry.FileName };
                    }

                    item.Init(fileData);

                    folder.AddItem(item);
                }
            }
        }
        public int GetDocumentsCount() {
            if (Package.IsNull()) { throw new WarningException("Please, load some archival package first!"); }
            return GetDocumentsCount(Package.Documents);
        }
        public IEnumerable<ItemBase> GetAllFiles() {
            var items = new List<ItemBase>();
            if (Package.IsNotNull()) {
                items.AddRange(GetAllFiles(Package.Documents));
                items.AddRange(GetAllFiles(Package.Metadata));
                items.AddRange(GetAllFiles(Package.Objects));
            }
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
                        if (itemName == Package.Documents.FolderName.ToLower()) {
                            folder = Package.Documents;
                            continue;
                        }
                        else if (itemName == Package.Metadata.FolderName.ToLower()) {
                            folder = Package.Metadata;
                            continue;
                        }
                        else if (itemName == Package.Objects.FolderName.ToLower()) {
                            folder = Package.Objects;
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
        public FolderBase GetParentFolder(ItemBase item) {
            if (item.IsNotNull() && item.FilePath.IsNotNullOrEmpty()) {
                return GetParentFolder(item.FilePath);
            }
            return default;
        }
        public FolderBase GetParentFolder(string filePath) {
            if (filePath.IsNotNullOrEmpty()) {
                FolderBase folder = null;
                var table = filePath.Split('/');

                for (int i = 0; i < table.Length; i++) {
                    var itemName = table[i].ToLower();
                    if (i == 0) {
                        if (itemName == Package.Documents.FolderName.ToLower()) {
                            folder = Package.Documents;
                            continue;
                        }
                        else if (itemName == Package.Metadata.FolderName.ToLower()) {
                            folder = Package.Metadata;
                            continue;
                        }
                        else if (itemName == Package.Objects.FolderName.ToLower()) {
                            folder = Package.Objects;
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

                return folder;
            }
            return default;
        }

        private int GetDocumentsCount(DocumentFolder folder) {
            var count = 0;
            if (folder.IsNotNull()) {
                if (folder.Items.IsNotNull()) { count += folder.Items.Count; }
                if (folder.Folders.IsNotNull()) {
                    foreach (var subFolder in folder.Folders) {
                        count += GetDocumentsCount(subFolder);
                    }
                }
            }
            return count;
        }
        private void AddMetadata(MetdataFolder folder, ZipFile zipFile, string folderPath = null) {
            if (folder.IsNull()) { return; }
            if (zipFile.IsNull()) { throw new ArgumentNullException("zipFile"); }
            if (folderPath.IsNullOrEmpty()) { folderPath = folder.FolderName; }
            if (folderPath.IsNullOrEmpty()) { throw new NullReferenceException("FolderPath and folder name is empty!"); }

            var converter = new XmlConverter();
            foreach (var item in folder.Items) {
                zipFile.AddEntry($"{folderPath}/{item.FileName}", converter.GetXml(item.Document).ToByteArray());
            }

            if (folder.Folders.IsNotNull() && folder.Folders.Count > 0) {
                foreach (var item in folder.Folders) {
                    AddMetadata(item, zipFile, $"{folderPath}/{item.FolderName}");
                }
            }
        }
        private void AddDocuments(DocumentFolder folder, ZipFile zipFile, string folderPath = null) {
            if (folder.IsNull()) { return; }
            if (zipFile.IsNull()) { throw new ArgumentNullException("zipFile"); }
            if (folderPath.IsNullOrEmpty()) { folderPath = folder.FolderName; }
            if (folderPath.IsNullOrEmpty()) { throw new NullReferenceException("FolderPath and folder name is empty!"); }

            foreach (var item in folder.Items) {
                zipFile.AddEntry($"{folderPath}/{item.FileName}", item.FileData);
            }

            if (folder.Folders.IsNotNull() && folder.Folders.Count > 0) {
                foreach (var item in folder.Folders) {
                    AddDocuments(item, zipFile, $"{folderPath}/{item.FolderName}");
                }
            }
        }
        private void InitializePackage() {
            Package = new Package() {
                Documents = new DocumentFolder() {
                    FolderName = MainDirectoriesName.Files.GetXmlEnum(),
                    Items = new List<DocumentFile>(),
                    Folders = new List<DocumentFolder>()
                },
                Metadata = new MetdataFolder() {
                    FolderName = MainDirectoriesName.Metadata.GetXmlEnum(),
                    Items = new List<MetadataFile>(),
                    Folders = new List<MetdataFolder>()
                },
                Objects = new MetdataFolder() {
                    FolderName = MainDirectoriesName.Objects.GetXmlEnum(),
                    Items = new List<MetadataFile>(),
                    Folders = new List<MetdataFolder>()
                }
            };
        }
        private string GetValidFileName(string filePath) {
            if (filePath.IsNull()) { throw new NullReferenceException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            var result = Path.GetFileName(filePath);
            result = result.RemoveIllegalCharacters();
            result = result.RemovePolishChars();
            return result;
        }
        private bool IsPackageValid(ZipFile zipFile) {
            var zipFileHasMandatoryDirectories =
               zipFile.EntryFileNames.Where(x => x.StartsWith(MainDirectoriesName.Files.GetXmlEnum())).Count() > 0 &&
               zipFile.EntryFileNames.Where(x => x.StartsWith(MainDirectoriesName.Metadata.GetXmlEnum())).Count() > 0 &&
               zipFile.EntryFileNames.Where(x => x.StartsWith(MainDirectoriesName.Objects.GetXmlEnum())).Count() > 0;

            if (!zipFileHasMandatoryDirectories) { throw new PackageInvalidException(); }
            return default;
        }
    }
}
