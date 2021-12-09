/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.ArchivalPackage.Exceptions;
using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Validators;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Abc.Nes.ArchivalPackage.Validators {
    public class PackageValidator : IPackageValidator {
        public void Dispose() { }

        public IValidationResult GetValidationResult(string filePath, bool validateMetdataFiles = false, bool breakOnFirstError = true) {
            Exception exception;
            var package = GetPackage(filePath, out exception);
            if (breakOnFirstError && exception != null) {
                throw exception;
            }
            return GetValidationResult(package, validateMetdataFiles, breakOnFirstError);
        }

        public IValidationResult GetValidationResult(Package package, bool validateMetdataFiles = false, bool breakOnFirstError = true) {
            IValidationResult result = new PackageValidationResult();
            var resx = Properties.Default.ResourceManager;
            if (CultureInfo.CurrentCulture.Name == "pl" || CultureInfo.CurrentCulture.Name == "pl-PL") {
                resx = Properties.Polish.ResourceManager;
            }

            if (package.IsNull()) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("eADM"),
                    Name = resx.GetString("Package"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.Incorrect,
                    DefaultMessage = resx.GetString("PackageIsNotInitialized")
                });
                return result;
            }
            if (package.Documents.IsNull()) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Documents"),
                    Name = resx.GetString("Package"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.Incorrect,
                    DefaultMessage = resx.GetString("PackageHasNoDocumentsFolder")
                });
            }
            if (package.Metadata.IsNull()) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Metadata"),
                    Name = resx.GetString("Package"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.Incorrect,
                    DefaultMessage = resx.GetString("PackageHasNoMetadataFolder")
                });
            }


            if (package.Documents.IsNotNull() && package.Documents.IsEmpty) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Documents"),
                    Name = resx.GetString("Documents"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.HasNoElements,
                    DefaultMessage = resx.GetString("PackageHasNoDocuments")
                });
            }

            if (package.Metadata.IsNotNull() && package.Metadata.IsEmpty) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Metadata"),
                    Name = resx.GetString("Metadata"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.HasNoElements,
                    DefaultMessage = resx.GetString("PackageHasNoMetadata")
                });
            }

            if (!CheckDocumentAndMetadataCount(package)) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Metadata"),
                    Name = resx.GetString("Metadata"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.Incorrect,
                    DefaultMessage = resx.GetString("NumberOfMetadataIsInsufficient")
                });
            }

            IDocumentValidator validator = new DocumentValidator();

            foreach (var item in package.GetAllFiles(package.Documents)) {
                var metadata = package.GetMetadataFile(item);
                if (metadata.IsNull()) {
                    result.Add(new PackageValidationResultItem()
                    {
                        FullName = item.FilePath,
                        Name = item.FileName,
                        Source = ValidationResultSource.Metadata,
                        Type = ValidationResultType.NotFound,
                        DefaultMessage = String.Format(resx.GetString("MetadataNotFound"), item.FilePath)
                    });

                    if (breakOnFirstError) { break; }
                }
                else if (validateMetdataFiles) {
                    var metadataResult = validator.Validate(metadata.Document);
                    if (!metadataResult.IsCorrect) {
                        result.Add(new PackageValidationResultItem()
                        {
                            FullName = item.FilePath,
                            Name = item.FileName,
                            Source = ValidationResultSource.Metadata,
                            Type = ValidationResultType.Incorrect,
                            DefaultMessage = String.Format(resx.GetString("MetadataAreNotValid"), item.FilePath)
                        });

                        foreach (var _item in metadataResult) {
                            result.Add(_item);
                        }

                        if (breakOnFirstError) { break; }
                    }
                }
            }


            if (package.Objects.IsNull()) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Objects"),
                    Name = resx.GetString("Package"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.Incorrect,
                    DefaultMessage = resx.GetString("PackageHasNoObjectsFolder")
                });
            }
            if (package.Objects.IsNotNull() && package.Objects.IsEmpty) {
                result.Add(new PackageValidationResultItem()
                {
                    FullName = resx.GetString("Package_Objects"),
                    Name = resx.GetString("Metadata"),
                    Source = ValidationResultSource.Package,
                    Type = ValidationResultType.HasNoElements,
                    DefaultMessage = resx.GetString("PackageHasNoObjects")
                });
            }

            return result;
        }

        public bool Validate(string filePath, out string message, bool validateMetdataFiles = false, bool breakOnFirstError = true) {
            Exception exception;
            var package = GetPackage(filePath, out exception);
            if (breakOnFirstError && exception != null) {
                throw exception;
            }
            return Validate(package, out message, validateMetdataFiles, breakOnFirstError);
        }

        public bool Validate(Package package, out string message, bool validateMetdataFiles = false, bool breakOnFirstError = true) {
            message = null;

            var resx = Properties.Default.ResourceManager;
            if (CultureInfo.CurrentCulture.Name == "pl" || CultureInfo.CurrentCulture.Name == "pl-PL") {
                resx = Properties.Polish.ResourceManager;
            }

            var messageBuilder = new StringBuilder();
            if (package.IsNull()) { messageBuilder.AppendLine(resx.GetString("PackageIsNotInitialized")); }
            if (package.IsNotNull() && package.Documents.IsNull()) { messageBuilder.AppendLine(resx.GetString("PackageIsNotInitialized")); }
            if (package.IsNotNull() && package.Documents.IsNotNull() && package.Documents.IsEmpty) { messageBuilder.AppendLine(resx.GetString("PackageHasNoDocuments")); }

            if (package.IsNotNull()) {
                IDocumentValidator validator = new DocumentValidator();
                foreach (var item in package.GetAllFiles(package.Documents)) {
                    var metadata = package.GetMetadataFile(item);
                    if (metadata.IsNull()) {
                        messageBuilder.AppendLine(String.Format(resx.GetString("MetadataNotFound"), item.FilePath));
                        if (breakOnFirstError) { break; }
                    }
                    else if (validateMetdataFiles) {
                        var _result = validator.Validate(metadata.Document);

                        if (_result.IsNotNull() && _result.Count > 0) {
                            messageBuilder.AppendLine(String.Format(resx.GetString("MetadataAreNotValid"), item.FilePath));
                            foreach (var _resultItem in _result) {
                                messageBuilder.AppendLine(_resultItem.DefaultMessage);
                            }
                            messageBuilder.AppendLine(String.Empty);
                            if (breakOnFirstError) { break; }
                        }
                    }
                }
            }

            if (package.IsNotNull() && package.Objects.IsNull()) { messageBuilder.AppendLine(resx.GetString("PackageHasNoObjectsFolder")); }
            if (package.IsNotNull() && package.Objects.IsEmpty) { messageBuilder.AppendLine(resx.GetString("PackageHasNoObjects")); }
            message = messageBuilder.ToString();
            return messageBuilder.Length == 0;
        }

        public bool IsPackageValid(Stream stream, out Exception exception) {
            exception = null;
            using (var zipFile = ZipFile.Read(stream, new ReadOptions()
            {
                Encoding = Encoding.UTF8
            })) {
                var zipFileHasMandatoryDirectories =
                   zipFile.EntryFileNames.Where(x => x.ToLower().StartsWith(MainDirectoriesName.Files.GetXmlEnum().ToLower())).Count() > 0 &&
                   zipFile.EntryFileNames.Where(x => x.ToLower().StartsWith(MainDirectoriesName.Metadata.GetXmlEnum().ToLower())).Count() > 0 &&
                   zipFile.EntryFileNames.Where(x => x.ToLower().StartsWith(MainDirectoriesName.Objects.GetXmlEnum().ToLower())).Count() > 0;

                if (!zipFileHasMandatoryDirectories) {
                    exception = new PackageInvalidException();
                    return false;
                }
            }
            return true;
        }
        public bool IsPackageValid(string filePath, out Exception exception) {
            exception = null;
            using (var zipFile = ZipFile.Read(filePath)) {
                var zipFileHasMandatoryDirectories =
                   zipFile.EntryFileNames.Where(x => x.ToLower().StartsWith(MainDirectoriesName.Files.GetXmlEnum().ToLower())).Count() > 0 &&
                   zipFile.EntryFileNames.Where(x => x.ToLower().StartsWith(MainDirectoriesName.Metadata.GetXmlEnum().ToLower())).Count() > 0 &&
                   zipFile.EntryFileNames.Where(x => x.ToLower().StartsWith(MainDirectoriesName.Objects.GetXmlEnum().ToLower())).Count() > 0;

                if (!zipFileHasMandatoryDirectories) {
                    exception = new PackageInvalidException();
                    return false;
                }
            }
            return true;
        }

        public Package GetPackage(string filePath, out Exception exception) {
            exception = null;
            if (filePath.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(filePath); }
            if (!ZipFile.IsZipFile(filePath)) { throw new ZipException("Specified file is not a zip file!"); }

            IsPackageValid(filePath, out exception);
            return InitializePackage(filePath);
        }

        public Package GetPackage(Stream stream, out Exception exception) {
            exception = null;
            if (stream.IsNull() || stream.Length == 0) { throw new ArgumentNullException(); }
            stream.Position = 0;
            if (!ZipFile.IsZipFile(stream, false)) { throw new ZipException("Specified file is not a zip file!"); }
            stream.Position = 0;
            IsPackageValid(stream, out exception);
            return InitializePackage();
        }

        public Package InitializePackage(string filePath = null) {
            return new Package(filePath)
            {
                Documents = new DocumentFolder()
                {
                    FolderName = MainDirectoriesName.Files.GetXmlEnum(),
                    Items = new List<DocumentFile>(),
                    Folders = new List<DocumentFolder>()
                },
                Metadata = new MetadataFolder()
                {
                    FolderName = MainDirectoriesName.Metadata.GetXmlEnum(),
                    Items = new List<MetadataFile>(),
                    Folders = new List<MetadataFolder>()
                },
                Objects = new MetadataFolder()
                {
                    FolderName = MainDirectoriesName.Objects.GetXmlEnum(),
                    Items = new List<MetadataFile>(),
                    Folders = new List<MetadataFolder>()
                },
                Another = new DocumentFolder() {
                    FolderName = MainDirectoriesName.Another.GetXmlEnum(),
                    Items = new List<DocumentFile>(),
                    Folders = new List<DocumentFolder>()
                }
            };
        }

        private bool CheckDocumentAndMetadataCount(Package package) {
            return CheckDocumentAndMetadataCount(package.Documents, package.Metadata);
        }
        private bool CheckDocumentAndMetadataCount(DocumentFolder folder, MetadataFolder metadataFolder) {
            if (folder.IsNull()) { return false; }
            if (metadataFolder.IsNull()) { return false; }

            foreach (var item in folder.Items) {
                if (!metadataFolder.Items.Where(x => x.FileName.Contains(item.FileName)).Any()) { return false; }
            }

            foreach (var item in folder.Folders) {
                if (metadataFolder.Items.Where(x => x.FileName == $"{item.FolderName}.xml").Any()) { continue; }
                var result = CheckDocumentAndMetadataCount(item, metadataFolder.Folders.Where(x => x.FolderName == item.FolderName).FirstOrDefault());
                if (!result) { return false; }
            }

            return true;
        }
    }
}
