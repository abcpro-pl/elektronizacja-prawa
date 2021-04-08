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
using Abc.Nes.ArchivalPackage.Validators;
using Abc.Nes.Converters;
using Abc.Nes.Validators;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Abc.Nes.ArchivalPackage {
    public class PackageManager : IPackageManager {
        public Enumerations.DocumentType PackageType { get; private set; } = Enumerations.DocumentType.None;
        public Package Package { get; private set; }
        public string FilePath { get; private set; }
        public void AddFile(DocumentFile document, IDocument metadata = null) {
            if (document.IsNull()) { throw new ArgumentNullException(); }
            if (Package.IsNull()) {
                using (IPackageValidator validator = new PackageValidator()) {
                    Package = validator.InitializePackage();
                }
            }
            Package.Documents.Items.Add(document);
            if (metadata.IsNotNull()) {
                Package.Metadata.Items.Add(new MetadataFile() {
                    Document = metadata,
                    FileName = $"{document.FileName}.xml"
                });
            }
        }
        public void AddFile(string filePath, IDocument metadata = null) {
            if (filePath.IsNull()) { throw new ArgumentNullException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            if (Package.IsNull()) {
                using (IPackageValidator validator = new PackageValidator()) {
                    Package = validator.InitializePackage();
                }
            }

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
        public void AddFiles(IEnumerable<DocumentFile> documents, string folderName = null, IEnumerable<IDocument> metadata = null) {
            if (documents.IsNull()) { throw new ArgumentNullException("documents"); }
            if (metadata.IsNotNull() && metadata.Count() != documents.Count()) {
                throw new WrongMetadataCountException();
            }
            if (Package.IsNull()) {
                using (IPackageValidator validator = new PackageValidator()) {
                    Package = validator.InitializePackage();
                }
            }

            var folder = Package.Documents;
            if (folderName.IsNotNullOrEmpty()) {
                folder = new DocumentFolder() {
                    FolderName = folderName,
                    Items = new List<DocumentFile>()
                };
                Package.Documents.Folders.Add(folder);
            }

            MetdataFolder metadataFolder = Package.Metadata;
            if (metadata.IsNotNull() && metadata.Count() > 0 && folderName.IsNotNullOrEmpty()) {
                metadataFolder = new MetdataFolder() {
                    FolderName = folderName,
                    Items = new List<MetadataFile>()
                };
                Package.Metadata.Folders.Add(metadataFolder);
            }

            for (int i = 0; i < documents.Count(); i++) {
                var document = documents.ToArray()[i];
                document.FileName = document.FileName.RemoveIllegalCharacters().RemovePolishChars();
                folder.Items.Add(document);
                if (metadata.IsNotNull()) {
                    var fileMetadata = metadata.ToArray()[i];
                    metadataFolder.Items.Add(new MetadataFile() {
                        Document = fileMetadata,
                        FileName = $"{document.FileName}.xml"
                    });
                }
            }
        }
        public void AddFiles(IEnumerable<string> files, string folderName = null, IEnumerable<IDocument> metadata = null) {
            if (files.IsNull()) { throw new ArgumentNullException("files"); }

            foreach (var filePath in files) {
                if (filePath.IsNull()) { throw new NullReferenceException(); }
                if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
            }

            if (metadata.IsNotNull() && metadata.Count() != files.Count()) {
                throw new WrongMetadataCountException();
            }

            if (Package.IsNull()) {
                using (IPackageValidator validator = new PackageValidator()) {
                    Package = validator.InitializePackage();
                }
            }

            var folder = Package.Documents;
            if (folderName.IsNotNullOrEmpty()) {
                folder = new DocumentFolder() {
                    FolderName = folderName,
                    Items = new List<DocumentFile>()
                };
                Package.Documents.Folders.Add(folder);
            }

            MetdataFolder metadataFolder = Package.Metadata;
            if (metadata.IsNotNull() && folderName.IsNotNullOrEmpty()) {
                metadataFolder = new MetdataFolder() {
                    FolderName = folderName,
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
        }
        public void AddObject(IDocument metadata, string fileName) {
            if (metadata.IsNull()) { throw new ArgumentNullException("metadata"); }
            if (fileName.IsNullOrEmpty()) { throw new ArgumentNullException("fileName"); }
            if (!fileName.ToLower().EndsWith(".xml")) { throw new Exception("The Object file must by an XML file!"); }

            if (Package.IsNull()) {
                using (IPackageValidator validator = new PackageValidator()) {
                    Package = validator.InitializePackage();
                }
            }

            Package.Objects.AddItem(new MetadataFile() {
                Document = metadata,
                FileName = fileName
            });
        }
        public void Save(string filePath = null, bool appendFileDataOnly = false) {
            if (Package.IsNull()) { throw new NullReferenceException("Package is not initialized!"); }
            if (Package.IsEmpty) { throw new PackageIsEmptyException(); }
            if (filePath.IsNullOrEmpty() && FilePath.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            if (filePath.IsNotNullOrEmpty()) { FilePath = filePath; }

            var tmp = Path.Combine(Path.GetTempPath(), $"{string.Empty.GenerateId()}.zip");
            using (var zipFile = new ZipFile(tmp)) {
                AddDocuments(Package.Documents, zipFile);
                AddMetadata(Package.Metadata, zipFile, appendFileDataOnly: appendFileDataOnly);
                AddMetadata(Package.Objects, zipFile, appendFileDataOnly: appendFileDataOnly);

                zipFile.Save();
            }

            if (File.Exists(tmp)) { File.Copy(tmp, FilePath, true); }

            try { if (File.Exists(tmp)) { File.Delete(tmp); } } catch { }
        }
        public Stream Save(bool appendFileDataOnly = false) {
            if (Package.IsNull()) { throw new NullReferenceException("Package is not initialized!"); }
            if (Package.IsEmpty) { throw new PackageIsEmptyException(); }
            var stream = new MemoryStream();
            using (var zipFile = new ZipFile()) {
                AddDocuments(Package.Documents, zipFile);
                AddMetadata(Package.Metadata, zipFile, appendFileDataOnly: appendFileDataOnly);
                AddMetadata(Package.Objects, zipFile, appendFileDataOnly: appendFileDataOnly);

                zipFile.Save(stream);
            }

            return stream;
        }
        public void LoadPackage(Stream stream) {
            // Package is already loaded
            if (Package.IsNotNull() && Package.Documents.IsNotNull() && !Package.Documents.IsEmpty) { return; }

            using (IPackageValidator validator = new PackageValidator()) {
                Package = validator.GetPackage(stream);
            }
            stream.Position = 0;
            using (var zipFile = ZipFile.Read(stream, new ReadOptions() {
                Encoding = Encoding.UTF8
            })) {
                LoadPackageEntries(zipFile);
            }
        }
        public void LoadPackage(string filePath) {
            // Package is already loaded
            if (FilePath == filePath && Package.IsNotNull() && Package.Documents.IsNotNull() && !Package.Documents.IsEmpty) { return; }

            using (IPackageValidator validator = new PackageValidator()) {
                Package = validator.GetPackage(filePath);
                if (Package.IsNotNull()) { FilePath = Package.FilePath; }
            }

            using (var zipFile = ZipFile.Read(filePath)) {
                LoadPackageEntries(zipFile);
            }
        }
        public int GetDocumentsCount() {
            if (Package.IsNull()) { throw new WarningException("Please, load some archival package first!"); }
            return GetDocumentsCount(Package.Documents);
        }
        public IEnumerable<ItemBase> GetAllFiles() {
            return Package.GetAllFiles();
        }
        public IEnumerable<ItemBase> GetAllFiles(FolderBase folder) {
            return Package.GetAllFiles(folder);
        }
        public IEnumerable<DocumentFile> GetAllDocuments() {
            return Package.GetAllFiles(Package.Documents).OfType<DocumentFile>();
        }
        public MetadataFile GetMetadataFile(ItemBase documentFile) {
            if (documentFile.IsNotNull() && documentFile.FilePath.IsNotNullOrEmpty()) {
                var regex = new Regex(Regex.Escape($"{MainDirectoriesName.Files.GetXmlEnum()}"), RegexOptions.IgnoreCase);
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
        public bool Validate(out string message, bool validateMetdataFiles = false, bool breakOnFirstError = true) {
            using (IPackageValidator validator = new PackageValidator()) {
                return validator.Validate(Package, out message, validateMetdataFiles, breakOnFirstError);
            }
        }
        public IValidationResult GetValidationResult(bool validateMetdataFiles = false, bool breakOnFirstError = true) {
            using (IPackageValidator validator = new PackageValidator()) {
                return validator.GetValidationResult(Package, validateMetdataFiles, breakOnFirstError);
            }
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
        private void AddMetadata(MetdataFolder folder, ZipFile zipFile, string folderPath = null, bool appendFileDataOnly = false) {
            if (folder.IsNull()) { return; }
            if (zipFile.IsNull()) { throw new ArgumentNullException("zipFile"); }
            if (folderPath.IsNullOrEmpty()) { folderPath = folder.FolderName; }
            if (folderPath.IsNullOrEmpty()) { throw new NullReferenceException("FolderPath and folder name is empty!"); }

            var converter = new XmlConverter();
            foreach (var item in folder.Items) {
                if (appendFileDataOnly && item.FileData.IsNotNull()) {
                    zipFile.AddEntry($"{folderPath}/{item.FileName}", item.FileData);
                }
                else {
                    zipFile.AddEntry($"{folderPath}/{item.FileName}", converter.GetXml(item.Document).ToByteArray());
                }
            }

            if (folder.Folders.IsNotNull() && folder.Folders.Count > 0) {
                foreach (var item in folder.Folders) {
                    AddMetadata(item, zipFile, $"{folderPath}/{item.FolderName}", appendFileDataOnly);
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
        private string GetValidFileName(string filePath) {
            if (filePath.IsNull()) { throw new NullReferenceException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            var result = Path.GetFileName(filePath);
            result = result.RemoveIllegalCharacters();
            result = result.RemovePolishChars();
            return result;
        }
        private void LoadPackageEntries(ZipFile zipFile) {
            foreach (var entry in zipFile.Entries) {
                if (entry.Attributes == FileAttributes.Directory) {
                    var dirName = entry.FileName;
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;

                    if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }

                    foreach (var dir in dirs) {
                        if (dir.ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower() ||
                            dir.ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                            dir.ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { continue; }

                        folder = folder.CreateSubFolder(dir);
                    }
                }
                else {
                    var dirName = Path.GetDirectoryName(entry.FileName).Replace("\\", "/");
                    var fileName = Path.GetFileName(entry.FileName);
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;
                    if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
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
                    if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) {
                        item = new DocumentFile() { FileName = fileName, FilePath = entry.FileName };
                    }
                    else {
                        item = new MetadataFile() { FileName = fileName, FilePath = entry.FileName };
                    }

                    item.Init(fileData);

                    if (item is MetadataFile && PackageType == Enumerations.DocumentType.None) {
                        try {
                            PackageType = (item as MetadataFile).Document.DocumentType;
                        }
                        catch { }
                    }

                    folder.AddItem(item);
                }
            }
        }

        public void Dispose() { }
    }
}