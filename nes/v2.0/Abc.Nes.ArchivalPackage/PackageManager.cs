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
using Abc.Nes.Elements;
using Abc.Nes.Enumerations;
using Abc.Nes.Exceptions;
using Abc.Nes.Validators;
using Ionic.Zip;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

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

            MetadataFolder metadataFolder = Package.Metadata;
            if (metadata.IsNotNull() && metadata.Count() > 0 && folderName.IsNotNullOrEmpty()) {
                metadataFolder = new MetadataFolder() {
                    FolderName = folderName,
                    Items = new List<MetadataFile>()
                };
                Package.Metadata.Folders.Add(metadataFolder);

                // compile and add folder metadata file
                var folderMetadata = new MetadataFile() {
                    FileName = $"{folderName}.xml"
                };

                if (metadata.First().DocumentType == Enumerations.DocumentType.Nes16) {
                    folderMetadata.Document = new Document16() {
                        Formats = new List<Elements.FormatElement16> {
                            new Elements.FormatElement16() { Type = "multipart/mixed" }
                        },
                        Description = $"Metadane folderu {folder}"
                    };

                    foreach (var _metadata in metadata) {
                        var m = _metadata as Document16;
                        var d = folderMetadata.Document as Document16;
                        if (d.Recipients.IsNull()) { d.Recipients = new List<Elements.RecipientElement16>(); }
                        foreach (var item in m.Recipients) {
                            if (!d.Recipients.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Recipients.Add(item);
                            }
                        }

                        if (d.Dates.IsNull()) { d.Dates = new List<Elements.DateElement16>(); }
                        foreach (var item in m.Dates) {
                            if (!d.Dates.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Dates.Add(item);
                            }
                        }

                        if (d.Access.IsNull()) { d.Access = new List<Elements.AccessElement16>(); }
                        foreach (var item in m.Access) {
                            if (!d.Access.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Access.Add(item);
                            }
                        }

                        if (d.Groupings.IsNull()) { d.Groupings = new List<Elements.GroupingElement>(); }
                        foreach (var item in m.Groupings) {
                            if (!d.Groupings.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Groupings.Add(item);
                            }
                        }

                        if (d.Identifiers.IsNull()) { d.Identifiers = new List<Elements.IdentifierElement16>(); }
                        foreach (var item in m.Identifiers) {
                            if (!d.Identifiers.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Identifiers.Add(item);
                            }
                        }

                        if (d.Authors.IsNull()) { d.Authors = new List<Elements.AuthorElement16>(); }
                        foreach (var item in m.Authors) {
                            if (!d.Authors.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Authors.Add(item);
                            }
                        }

                        if (d.Types.IsNull()) { d.Types = new List<Elements.TypeElement16>(); }
                        foreach (var item in m.Types) {
                            if (!d.Types.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Types.Add(item);
                            }
                        }

                        if (d.Titles.IsNull()) { d.Titles = new List<Elements.TitleElement>(); }
                        foreach (var item in m.Titles) {
                            if (!d.Titles.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Titles.Add(item);
                            }
                        }

                        if (d.Relations.IsNull()) { d.Relations = new List<Elements.RelationElement16>(); }
                        foreach (var item in m.Relations) {
                            if (!d.Relations.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Relations.Add(item);
                            }
                        }

                        if (d.Languages.IsNull()) { d.Languages = new List<Elements.LanguageElement>(); }
                        foreach (var item in m.Languages) {
                            if (!d.Languages.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Languages.Add(item);
                            }
                        }
                    }
                }
                else if (metadata.First().DocumentType == Enumerations.DocumentType.Nes17) {

                    folderMetadata.Document = new Document17() {
                        Formats = new List<Elements.FormatElement17> {
                            new Elements.FormatElement17() { Type = "multipart/mixed" }
                        },
                        Description = $"Metadane folderu {folder}"
                    };

                    foreach (var _metadata in metadata) {
                        var m = _metadata as Document17;
                        var d = folderMetadata.Document as Document17;
                        if (d.Recipients.IsNull()) { d.Recipients = new List<Elements.RecipientElement17>(); }
                        foreach (var item in m.Recipients) {
                            if (!d.Recipients.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Recipients.Add(item);
                            }
                        }

                        if (d.Dates.IsNull()) { d.Dates = new List<Elements.DateElement17>(); }
                        foreach (var item in m.Dates) {
                            if (!d.Dates.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Dates.Add(item);
                            }
                        }

                        if (d.Access.IsNull()) { d.Access = new List<Elements.AccessElement17>(); }
                        foreach (var item in m.Access) {
                            if (!d.Access.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Access.Add(item);
                            }
                        }

                        if (d.Groupings.IsNull()) { d.Groupings = new List<Elements.GroupingElement>(); }
                        foreach (var item in m.Groupings) {
                            if (!d.Groupings.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Groupings.Add(item);
                            }
                        }

                        if (d.Identifiers.IsNull()) { d.Identifiers = new List<Elements.IdentifierElement>(); }
                        foreach (var item in m.Identifiers) {
                            if (!d.Identifiers.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Identifiers.Add(item);
                            }
                        }

                        if (d.Authors.IsNull()) { d.Authors = new List<Elements.AuthorElement17>(); }
                        foreach (var item in m.Authors) {
                            if (!d.Authors.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Authors.Add(item);
                            }
                        }

                        if (d.Types.IsNull()) { d.Types = new List<Elements.TypeElement17>(); }
                        foreach (var item in m.Types) {
                            if (!d.Types.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Types.Add(item);
                            }
                        }

                        if (d.Titles.IsNull()) { d.Titles = new List<Elements.TitleElement>(); }
                        foreach (var item in m.Titles) {
                            if (!d.Titles.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Titles.Add(item);
                            }
                        }

                        if (d.Relations.IsNull()) { d.Relations = new List<Elements.RelationElement>(); }
                        foreach (var item in m.Relations) {
                            if (!d.Relations.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Relations.Add(item);
                            }
                        }

                        if (d.Languages.IsNull()) { d.Languages = new List<Elements.LanguageElement>(); }
                        foreach (var item in m.Languages) {
                            if (!d.Languages.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Languages.Add(item);
                            }
                        }
                    }
                }
                else if (metadata.First().DocumentType == Enumerations.DocumentType.Nes20) {
                    folderMetadata.Document = new Document() {
                        Formats = new List<Elements.FormatElement> {
                            new Elements.FormatElement() { Type = "multipart/mixed" }
                        },
                        Description = $"Metadane folderu {folder}"
                    };

                    foreach (var _metadata in metadata) {
                        var m = _metadata as Document;
                        var d = folderMetadata.Document as Document;

                        if (d.Senders.IsNull()) { d.Senders = new List<Elements.SenderElement>(); }
                        foreach (var item in m.Senders) {
                            if (!d.Senders.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Senders.Add(item);
                            }
                        }

                        if (d.Recipients.IsNull()) { d.Recipients = new List<Elements.RecipientElement>(); }
                        foreach (var item in m.Recipients) {
                            if (!d.Recipients.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Recipients.Add(item);
                            }
                        }

                        if (d.Dates.IsNull()) { d.Dates = new List<Elements.DateElement>(); }
                        foreach (var item in m.Dates) {
                            if (!d.Dates.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Dates.Add(item);
                            }
                        }

                        if (d.Access.IsNull()) { d.Access = new List<Elements.AccessElement>(); }
                        foreach (var item in m.Access) {
                            if (!d.Access.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Access.Add(item);
                            }
                        }

                        if (d.Groupings.IsNull()) { d.Groupings = new List<Elements.GroupingElement>(); }
                        foreach (var item in m.Groupings) {
                            if (!d.Groupings.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Groupings.Add(item);
                            }
                        }

                        if (d.Identifiers.IsNull()) { d.Identifiers = new List<Elements.IdentifierElement>(); }
                        foreach (var item in m.Identifiers) {
                            if (!d.Identifiers.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Identifiers.Add(item);
                            }
                        }

                        if (d.Authors.IsNull()) { d.Authors = new List<Elements.AuthorElement>(); }
                        foreach (var item in m.Authors) {
                            if (!d.Authors.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Authors.Add(item);
                            }
                        }

                        if (d.Types.IsNull()) { d.Types = new List<Elements.TypeElement>(); }
                        foreach (var item in m.Types) {
                            if (!d.Types.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Types.Add(item);
                            }
                        }

                        if (d.Titles.IsNull()) { d.Titles = new List<Elements.TitleElement>(); }
                        foreach (var item in m.Titles) {
                            if (!d.Titles.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Titles.Add(item);
                            }
                        }

                        if (d.Relations.IsNull()) { d.Relations = new List<Elements.RelationElement>(); }
                        foreach (var item in m.Relations) {
                            if (!d.Relations.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Relations.Add(item);
                            }
                        }

                        if (d.Languages.IsNull()) { d.Languages = new List<Elements.LanguageElement>(); }
                        foreach (var item in m.Languages) {
                            if (!d.Languages.Where(x => x.ToString() == item.ToString()).Any()) {
                                d.Languages.Add(item);
                            }
                        }
                    }
                }

                Package.Metadata.AddItem(folderMetadata);
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

            MetadataFolder metadataFolder = Package.Metadata;
            if (metadata.IsNotNull() && folderName.IsNotNullOrEmpty()) {
                metadataFolder = new MetadataFolder() {
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
        public void AddPackageMetadata(IDocument metadata, string fileName = "metadane_paczki.xml") {
            if (metadata.IsNull()) { throw new ArgumentNullException("metadata"); }
            if (fileName.IsNullOrEmpty()) { throw new ArgumentNullException("fileName"); }
            if (!fileName.ToLower().EndsWith(".xml")) { throw new Exception("The Object file must by an XML file!"); }

            if (Package.IsNull()) {
                using (IPackageValidator validator = new PackageValidator()) {
                    Package = validator.InitializePackage();
                }
            }

            Package.PackageMetadata = new MetadataFile() {
                Document = metadata,
                FileName = fileName
            };
        }
        public void Save(string filePath = null, bool appendFileDataOnly = false) {
            if (Package.IsNull()) { throw new NullReferenceException("Package is not initialized!"); }
            if (Package.IsEmpty) { throw new PackageIsEmptyException(); }
            if (filePath.IsNullOrEmpty() && FilePath.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            if (filePath.IsNotNullOrEmpty()) { FilePath = filePath; }

            var tmp = Path.Combine(Path.GetTempPath(), $"{string.Empty.GenerateId()}.zip");
#if NET48
            using (var zipFile = new Ionic.Zip.ZipFile(tmp)) {
                AddDocuments(Package.Documents, zipFile);
                AddMetadata(Package.Metadata, zipFile, appendFileDataOnly: appendFileDataOnly);
                AddMetadata(Package.Objects, zipFile, appendFileDataOnly: appendFileDataOnly);

                zipFile.Save();
            }
#else
            using (var zipFile = System.IO.Compression.ZipFile.Open(tmp, ZipArchiveMode.Update)) {
                AddDocuments(Package.Documents, zipFile);
                AddMetadata(Package.Metadata, zipFile, appendFileDataOnly: appendFileDataOnly);
                AddMetadata(Package.Objects, zipFile, appendFileDataOnly: appendFileDataOnly);
                AddPackageMetadata(Package.PackageMetadata, zipFile, appendFileDataOnly: appendFileDataOnly);
            }
#endif

            if (File.Exists(tmp)) { File.Copy(tmp, FilePath, true); }

            try { if (File.Exists(tmp)) { File.Delete(tmp); } } catch { }
        }
        public Stream Save(bool appendFileDataOnly = false) {
            if (Package.IsNull()) { throw new NullReferenceException("Package is not initialized!"); }
            if (Package.IsEmpty) { throw new PackageIsEmptyException(); }
            var stream = new MemoryStream();
#if NET48
            using (var zipFile = new Ionic.Zip.ZipFile()) {
                AddDocuments(Package.Documents, zipFile);
                AddMetadata(Package.Metadata, zipFile, appendFileDataOnly: appendFileDataOnly);
                AddMetadata(Package.Objects, zipFile, appendFileDataOnly: appendFileDataOnly);

                zipFile.Save(stream);
            }
#else
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true)) {
                AddDocuments(Package.Documents, zip);
                AddMetadata(Package.Metadata, zip, appendFileDataOnly: appendFileDataOnly);
                AddMetadata(Package.Objects, zip, appendFileDataOnly: appendFileDataOnly);
                AddPackageMetadata(Package.PackageMetadata, zip, appendFileDataOnly: appendFileDataOnly);

                stream.Position = 0;
            }
#endif
            return stream;
        }
        public void LoadPackage(Stream stream, out Exception exception) {
            exception = null;

            // Package is already loaded
            if (Package.IsNotNull() && Package.Documents.IsNotNull() && !Package.Documents.IsEmpty) { return; }

            using (IPackageValidator validator = new PackageValidator()) {
                Package = validator.GetPackage(stream, out exception);
            }
            stream.Position = 0;
#if NET48
            using (var zipFile = Ionic.Zip.ZipFile.Read(stream, new ReadOptions() {
                Encoding = Console.OutputEncoding
            })) {
                LoadPackageEntries(zipFile, out var exception2);
                if (exception.IsNull() && exception2.IsNotNull()) { exception = exception2; }
            }
#else
            //using (var zipFile = new ZipArchive(stream, ZipArchiveMode.Read, true)) {
            //    LoadPackageEntries(zipFile, out exception);
            //}
            var encoding = Encoding.GetEncoding(1250); //polish
            ReaderOptions readerOptions = new ReaderOptions {
                ArchiveEncoding = new SharpCompress.Common.ArchiveEncoding {
                    Default = encoding,
                    Forced = null
                }
            };
            using (IArchive arch = ArchiveFactory.Open(stream, readerOptions)) {
                var type = ZipValidator.DetectArchiveType(stream);
                if (type == ZipValidator.ArchiveType.TarGz) {
                    using (var gzipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
                    using (var decompressStream = new MemoryStream()) {
                        gzipStream.CopyTo(decompressStream);
                        decompressStream.Position = 0;
                        using (var tarArchive = SharpCompress.Archives.Tar.TarArchive.Open(decompressStream)) {

                            LoadPackageEntries(tarArchive, out var ex2);
                            if (exception.IsNull() && ex2.IsNotNull()) exception = ex2;
                        }
                    }
                }
                else {
                    LoadPackageEntries(arch, out var ex2);
                    if (exception.IsNull() && ex2.IsNotNull()) exception = ex2;
                }
            }
#endif
        }
        public void LoadPackage(string filePath, out Exception exception) {
            exception = null;

            // Package is already loaded
            if (FilePath == filePath && Package.IsNotNull() && Package.Documents.IsNotNull() && !Package.Documents.IsEmpty) { return; }

            using (IPackageValidator validator = new PackageValidator()) {
                Package = validator.GetPackage(filePath, out exception);
                if (Package.IsNotNull()) { FilePath = Package.FilePath; }
            }
#if NET48
            ReadOptions opts = new ReadOptions() {
                Encoding = Console.OutputEncoding
            };
            using (var zipFile = Ionic.Zip.ZipFile.Read(filePath, opts)) {
                LoadPackageEntries(zipFile, out var exception2);
                if (exception.IsNull() && exception2.IsNotNull()) { exception = exception2; }
            }
#else
            //using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            //using (var zipFile = new ZipArchive(stream, ZipArchiveMode.Read, true)) {
            //    LoadPackageEntries(zipFile, out var ex2);
            //    if (exception.IsNull() && ex2.IsNotNull()) exception = ex2;
            //}
            var encoding = Encoding.GetEncoding(1250);
            //sharpcompress
            ReaderOptions readerOptions = new ReaderOptions {
                ArchiveEncoding = new SharpCompress.Common.ArchiveEncoding {
                    Default = encoding,
                    Forced = null
                }
            };

            if (FilePath.ToLower().EndsWith(".tar.gz")) {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {

                    using (var gzipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                    using (var decompressStream = new MemoryStream()) {
                        gzipStream.CopyTo(decompressStream);
                        decompressStream.Position = 0;
                        using (var tarArchive = SharpCompress.Archives.Tar.TarArchive.Open(decompressStream)) {

                            LoadPackageEntries(tarArchive, out var ex2);
                            if (exception.IsNull() && ex2.IsNotNull()) exception = ex2;
                        }
                    }
                }
            } 
            else {
                using (IArchive arch = ArchiveFactory.Open(filePath, readerOptions)) {
                    LoadPackageEntries(arch, out var ex2);
                    if (exception.IsNull() && ex2.IsNotNull()) exception = ex2;
                }
            }
#endif
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
            return Package.GetMetadataFile(documentFile);
        }
        public ItemBase GetItemByFilePath(string filePath) {
            return Package.GetItemByFilePath(filePath);
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
                        else if (itemName == Package.Another.FolderName.ToLower()) {
                            folder = Package.Another;
                            continue;
                        }
                        else {
                            // file in pseudo-another folder
                            folder = Package.Another.GetFolder(itemName, true);
                            if (folder.IsNotNull()) { continue; }
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

        private void AddMetadata(MetadataFolder folder, ZipArchive zipFile, string folderPath = null, bool appendFileDataOnly = false) {
            if (folder.IsNull()) { return; }
            if (zipFile.IsNull()) { throw new ArgumentNullException("zipFile"); }
            if (folderPath.IsNullOrEmpty()) { folderPath = folder.FolderName; }
            if (folderPath.IsNullOrEmpty()) { throw new NullReferenceException("FolderPath and folder name is empty!"); }

            var converter = new XmlConverter();
            foreach (var item in folder.Items) {
                if (appendFileDataOnly && item.FileData.IsNotNull()) {
                    var entry = zipFile.CreateEntry($"{folderPath}/{item.FileName}");
                    using (Stream entryStream = entry.Open()) {
                        entryStream.Write(item.FileData, 0, item.FileData.Length);
                    }
                }
                else {
                    var entry = zipFile.CreateEntry($"{folderPath}/{item.FileName}");
                    using(Stream entryStream = entry.Open()) {
                        var data = converter.GetXml(item.Document).ToByteArray();
                        entryStream.Write(data, 0, data.Length);
                    }
                }
            }

            if (folder.Folders.IsNotNull() && folder.Folders.Count > 0) {
                foreach (var item in folder.Folders) {
                    AddMetadata(item, zipFile, $"{folderPath}/{item.FolderName}", appendFileDataOnly);
                }
            }
        }

        private void AddPackageMetadata(MetadataFile metadataFile, ZipArchive zipFile, bool appendFileDataOnly = false) {
            if (metadataFile.IsNull()) { return; }
            if (zipFile.IsNull()) { throw new ArgumentNullException("zipFile"); }

            var converter = new XmlConverter();
            if (appendFileDataOnly && metadataFile.FileData.IsNotNull()) {
                var entry = zipFile.CreateEntry($"{metadataFile.FileName}");
                using (Stream entryStream = entry.Open()) {
                    entryStream.Write(metadataFile.FileData, 0, metadataFile.FileData.Length);
                }
            }
            else {
                var entry = zipFile.CreateEntry($"{metadataFile.FileName}");
                using (Stream entryStream = entry.Open()) {
                    var data = converter.GetXml(metadataFile.Document).ToByteArray();
                    entryStream.Write(data, 0, data.Length);
                }
            }
        }

        private void AddMetadata(MetadataFolder folder, Ionic.Zip.ZipFile zipFile, string folderPath = null, bool appendFileDataOnly = false) {
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

        private void AddDocuments(DocumentFolder folder, ZipArchive zipFile, string folderPath = null) {
            if (folder.IsNull()) { return; }
            if (zipFile.IsNull()) { throw new ArgumentNullException("zipFile"); }
            if (folderPath.IsNullOrEmpty()) { folderPath = folder.FolderName; }
            if (folderPath.IsNullOrEmpty()) { throw new NullReferenceException("FolderPath and folder name is empty!"); }

            foreach (var item in folder.Items) {
                var entry = zipFile.CreateEntry($"{folderPath}/{item.FileName}");
                using (Stream entryStream = entry.Open()) {
                    entryStream.Write(item.FileData, 0, item.FileData.Length);
                }
            }

            if (folder.Folders.IsNotNull() && folder.Folders.Count > 0) {
                foreach (var item in folder.Folders) {
                    AddDocuments(item, zipFile, $"{folderPath}/{item.FolderName}");
                }
            }
        }

        private void AddDocuments(DocumentFolder folder, Ionic.Zip.ZipFile zipFile, string folderPath = null) {
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
        private void LoadPackageEntries(Ionic.Zip.ZipFile zipFile, out Exception ex) {
            LoadPackageEntries2(zipFile.Entries, out ex);
        }

        private void LoadPackageEntries(IArchive archive, out Exception ex) {
            
            LoadPackageEntries2(archive.Entries, out ex);

        }
        private void LoadPackageEntries2(ICollection<ZipEntry> zipFileEntries, out Exception ex) {
            ex = null;
            foreach (var entry in zipFileEntries) {

                FileAttributes attributes = entry.Attributes;
                string entryFileName = entry.FileName;

                if (attributes == FileAttributes.Directory || attributes == (FileAttributes.Directory | FileAttributes.NotContentIndexed)) {
                    var dirName = entryFileName;
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;

                    if (dirs.Length == 0) { if (Package.Another.IsNull()) { Package.Another = new DocumentFolder() { FolderName = MainDirectoriesName.Another.GetXmlEnum().ToLower() }; } folder = Package.Another; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
                    else {
                        if (dirs.Length > 1) {
                            folder = Package.Another.GetFolder(dirs[0]);
                        }
                        else {
                            folder = Package.Another;
                        }
                    }

                    foreach (var dir in dirs) {
                        if (dir.ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower() ||
                            dir.ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                            dir.ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { continue; }
                        if (folder != null && folder.FolderName != dir) {
                            folder = folder.CreateSubFolder(dir);
                        }
                    }
                }
                else {
                    var dirName = Path.GetDirectoryName(entryFileName).Replace("\\", "/");
                    var fileName = Path.GetFileName(entryFileName);
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;
                    if (dirs.Length == 0) { folder = Package.Another; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Another.GetXmlEnum().ToLower()) { folder = Package.Another; }

                    foreach (var dir in dirs) {
                        if (dirs[0] == dir) {
                            if (folder.IsNull()) {
                                var __folder = Package.Another.GetFolder(dir);
                                if (__folder.IsNull()) { __folder = Package.Another.CreateSubFolder(dir); }
                                folder = __folder;
                            }
                            continue;
                        }
                        var _folder = folder.GetFolder(dir);
                        if (_folder.IsNull()) { _folder = folder.CreateSubFolder(dir); }
                        folder = _folder;
                    }

                    var stream = new MemoryStream();
                    entry.Extract(stream);
                    var fileData = stream.ToArray();

                    ItemBase item;
                    if (dirs.Length == 0) {
                        item = new DocumentFile() { FileName = fileName, FilePath = $"{MainDirectoriesName.Another.GetXmlEnum().ToLower()}/{entryFileName}" };
                    }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) {
                        item = new DocumentFile() { FileName = fileName, FilePath = entryFileName };
                    }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                        dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) {
                        item = new MetadataFile() { FileName = fileName, FilePath = entryFileName };
                    }
                    else {
                        item = new DocumentFile() { FileName = fileName, FilePath = $"{MainDirectoriesName.Another.GetXmlEnum().ToLower()}/{entryFileName}" };
                    }

                    item.Init(fileData, out ex);

                    if (ex != null) {
                        if (ex is System.Xml.XmlException) {
                            var exc = new System.Xml.XmlException($"Plik: {entryFileName}", ex);
                            ex = exc;
                        }
                        else if (ex is DocumentSchemaException) {
                            var excTitle = $"Plik: {entryFileName}\nSkładnia pliku {fileName} z folderu {dirName} nie jest zgodna z żadnym z wspieranych / obowiązujących schematów metadanych.";
                            var exc = new DocumentSchemaException(excTitle, ex);
                            ex = exc;
                        }
                        else {
                            var exc = new Exception($"Plik: {entryFileName}", ex);
                        }
                    }

                    if (item is MetadataFile && PackageType == Enumerations.DocumentType.None) {
                        try {
                            var itemMetadataFile = (item as MetadataFile);
                            if (itemMetadataFile.Document != null) {
                                PackageType = itemMetadataFile.Document.DocumentType;
                            }
                            else PackageType = Enumerations.DocumentType.None;
                        }
                        catch { }
                    }

                    folder.AddItem(item);
                }
            }
        }

        private void LoadPackageEntries2(IEnumerable<IArchiveEntry> zipFileEntries, out Exception ex) {
            ex = null;
            foreach (var entry in zipFileEntries) {
                try {
                    bool isDirectory = entry.IsDirectory;
                    string entryFileName = entry.Key.Replace('\\', '/');

                    if (isDirectory) {
                        var dirName = entryFileName;
                        var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        FolderBase folder = null;

                        if (dirs.Length == 0) { if (Package.Another.IsNull()) { Package.Another = new DocumentFolder() { FolderName = MainDirectoriesName.Another.GetXmlEnum().ToLower() }; } folder = Package.Another; }
                        else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                        else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                        else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
                        else {
                            if (dirs.Length > 1) {
                                folder = Package.Another.GetFolder(dirs[0]);
                            }
                            else {
                                folder = Package.Another;
                            }
                        }

                        foreach (var dir in dirs) {
                            if (dir.ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower() ||
                                dir.ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                                dir.ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { continue; }
                            if (folder != null && folder.FolderName != dir) {
                                folder = folder.CreateSubFolder(dir);
                            }
                        }
                    }
                    else {
                        //wczytaj plik metadane_paczki.xml jako metadane dla calej paczki
                        if (entryFileName == "metadane_paczki.xml") {
                            using (var stream = new MemoryStream()) {
                                using (var entryStream = entry.OpenEntryStream()) {
                                    entryStream.CopyTo(stream);
                                }
                                stream.Position = 0;
                                var fileData = stream.ToArray();

                                var metadataFile = new MetadataFile() {
                                    FileName = "metadane_paczki.xml",
                                    FilePath = entryFileName
                                };
                                metadataFile.Init(fileData, out ex);
                                Package.PackageMetadata = metadataFile;
                            }
                        } else {
                            var dirName = Path.GetDirectoryName(entryFileName).Replace("\\", "/");
                            var fileName = Path.GetFileName(entryFileName);
                            var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            FolderBase folder = null;
                            if (dirs.Length == 0) { folder = Package.Another; }
                            else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                            else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                            else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
                            else if (dirs[0].ToLower() == MainDirectoriesName.Another.GetXmlEnum().ToLower()) { folder = Package.Another; }

                            foreach (var dir in dirs) {
                                if (dirs[0] == dir) {
                                    if (folder.IsNull()) {
                                        var __folder = Package.Another.GetFolder(dir);
                                        if (__folder.IsNull()) { __folder = Package.Another.CreateSubFolder(dir); }
                                        folder = __folder;
                                    }
                                    continue;
                                }
                                var _folder = folder.GetFolder(dir);
                                if (_folder.IsNull()) { _folder = folder.CreateSubFolder(dir); }
                                folder = _folder;
                            }

                            // SharpCompress stream handling
                            using (var stream = new MemoryStream()) {
                                using (var entryStream = entry.OpenEntryStream()) {
                                    entryStream.CopyTo(stream);
                                }
                                stream.Position = 0;
                                var fileData = stream.ToArray();

                                ItemBase item;
                                if (dirs.Length == 0) {
                                    item = new DocumentFile() { FileName = fileName, FilePath = $"{MainDirectoriesName.Another.GetXmlEnum().ToLower()}/{entryFileName}" };
                                }
                                else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) {
                                    item = new DocumentFile() { FileName = fileName, FilePath = entryFileName };
                                }
                                else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                                    dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) {
                                    item = new MetadataFile() { FileName = fileName, FilePath = entryFileName };
                                }
                                else {
                                    item = new DocumentFile() { FileName = fileName, FilePath = $"{MainDirectoriesName.Another.GetXmlEnum().ToLower()}/{entryFileName}" };
                                }

                                item.Init(fileData, out ex);

                                if (ex != null) {
                                    if (ex is System.Xml.XmlException) {
                                        var exc = new System.Xml.XmlException($"Plik: {entryFileName}", ex);
                                        ex = exc;
                                    }
                                    else if (ex is DocumentSchemaException) {
                                        var excTitle = $"Plik: {entryFileName}\nSkładnia pliku {fileName} z folderu {dirName} nie jest " +
                                            $"zgodna z żadnym z wspieranych / obowiązujących schematów metadanych.";
                                        var exc = new DocumentSchemaException(excTitle, ex);
                                        ex = exc;
                                    }
                                    else {
                                        var exc = new Exception($"Plik: {entryFileName}", ex);
                                        ex = exc; // Fixed: assigning the new exception back to ex
                                    }
                                }

                                if (item is MetadataFile && PackageType == Enumerations.DocumentType.None) {
                                    try {
                                        var itemMetadataFile = (item as MetadataFile);
                                        if (itemMetadataFile.Document != null) {
                                            PackageType = itemMetadataFile.Document.DocumentType;
                                        }
                                        else PackageType = Enumerations.DocumentType.None;
                                    }
                                    catch { }
                                }

                                folder.AddItem(item);
                            }
                        }
                    }
                }
                catch (Exception extractEx) {
                    ex = new Exception($"Error extracting entry: {entry.Key}", extractEx);
                    break;
                }
            }
        }

        private bool IsFolder(ZipArchiveEntry entry) {
            return entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase);
            // || entry.FullName.EndsWith("\\", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadPackageEntries(ZipArchive zipFile, out Exception ex) {
            ex = null;
            foreach (var entry in zipFile.Entries) {
                if (IsFolder(entry)) {
                    var dirName = entry.FullName;
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;

                    if (dirs.Length == 0) { if (Package.Another.IsNull()) { Package.Another = new DocumentFolder() { FolderName = MainDirectoriesName.Another.GetXmlEnum().ToLower() }; } folder = Package.Another; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
                    else {
                        if (dirs.Length > 1) {
                            folder = Package.Another.GetFolder(dirs[0]);
                        }
                        else {
                            folder = Package.Another;
                        }
                    }

                    foreach (var dir in dirs) {
                        if (dir.ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower() ||
                            dir.ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                            dir.ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { continue; }
                        if (folder != null && folder.FolderName != dir) {
                            folder = folder.CreateSubFolder(dir);
                        }
                    }
                }
                else {
                    var dirName = Path.GetDirectoryName(entry.FullName).Replace("\\", "/");
                    var fileName = Path.GetFileName(entry.FullName); //FIXME: test, zakomentowac
                    fileName = entry.Name;
                    var dirs = dirName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    FolderBase folder = null;
                    if (dirs.Length == 0) { folder = Package.Another; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) { folder = Package.Documents; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower()) { folder = Package.Metadata; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) { folder = Package.Objects; }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Another.GetXmlEnum().ToLower()) { folder = Package.Another; }

                    foreach (var dir in dirs) {
                        if (dirs[0] == dir) {
                            if (folder.IsNull()) {
                                var __folder = Package.Another.GetFolder(dir);
                                if (__folder.IsNull()) { __folder = Package.Another.CreateSubFolder(dir); }
                                folder = __folder;
                            }
                            continue;
                        }
                        var _folder = folder.GetFolder(dir);
                        if (_folder.IsNull()) { _folder = folder.CreateSubFolder(dir); }
                        folder = _folder;
                    }

                    var stream = new MemoryStream();

                    using (var entryStream = entry.Open()) {
                        entryStream.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                    var fileData = stream.ToArray();

                    ItemBase item;
                    //FIXME: porownac name i fullname sciezki z windows
                    if (dirs.Length == 0) { 
                        item = new DocumentFile() { FileName = fileName, FilePath = $"{MainDirectoriesName.Another.GetXmlEnum().ToLower()}/{entry.FullName}" };
                    }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Files.GetXmlEnum().ToLower()) {
                        item = new DocumentFile() { FileName = fileName, FilePath = entry.FullName };
                    }
                    else if (dirs[0].ToLower() == MainDirectoriesName.Metadata.GetXmlEnum().ToLower() ||
                        dirs[0].ToLower() == MainDirectoriesName.Objects.GetXmlEnum().ToLower()) {
                        item = new MetadataFile() { FileName = fileName, FilePath = entry.FullName };
                    }
                    else {
                        item = new DocumentFile() { FileName = fileName, FilePath = $"{MainDirectoriesName.Another.GetXmlEnum().ToLower()}/{entry.FullName}" };
                    }

                    item.Init(fileData, out ex);

                    if (ex != null) {
                        if (ex is System.Xml.XmlException) {
                            var exc = new System.Xml.XmlException($"Plik: {entry.FullName}", ex);
                            ex = exc;
                        }
                        else {
                            var exc = new Exception($"Plik: {entry.FullName}", ex);
                        }
                    }

                    if (item is MetadataFile && PackageType == Enumerations.DocumentType.None) {
                        try {
                            var itemMetadataFile = (item as MetadataFile);
                            if (itemMetadataFile.Document != null) {
                                PackageType = itemMetadataFile.Document.DocumentType;
                            }
                            else PackageType = Enumerations.DocumentType.None;
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