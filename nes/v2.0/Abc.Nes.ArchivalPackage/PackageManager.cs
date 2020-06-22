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
using System.IO;
using System.Linq;

namespace Abc.Nes.ArchivalPackage {
    public class PackageManager {
        internal Model.Package Package { get; set; }

        public void AddFile(string filePath, Document metadata = null) {
            if (filePath.IsNull()) { throw new NullReferenceException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            if (Package.IsNull()) { InitializePackage(); }

            Package.Documents.Files.Add(new Model.DocumentFile() {
                FileName = GetValidFileName(filePath),
                FileData = File.ReadAllBytes(filePath)
            });

            if (metadata.IsNotNull()) {
                Package.Metdata.Metadata.Add(new Model.Metadata() {
                    Document = metadata,
                    FileName = $"{GetValidFileName(filePath)}.xml"
                });
            }
        }

        public void AddFiles(IEnumerable<string> files, string folderName, IEnumerable<Document> metadata = null) {
            if (files.IsNull()) { throw new NullReferenceException(); }
            if (folderName.IsNotNullOrEmpty()) { throw new NullReferenceException(); }

            if (files.Count() < 2) { throw new Exception("Wrong files count!"); }

            foreach (var filePath in files) {
                if (filePath.IsNull()) { throw new NullReferenceException(); }
                if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
            }

            if (metadata.IsNotNull() && metadata.Count() != files.Count()) {
                throw new Exception("Wrong metdata count!");
            }

            if (Package.IsNull()) { InitializePackage(); }
            var folder = new Model.DocumentSubFolder() {
                FolderName = folderName,
                Files = new List<Model.DocumentFile>()
            };
            Model.MetdataSubFolder metadataFolder = null;
            if (metadata.IsNotNull()) {
                metadataFolder = new Model.MetdataSubFolder() {
                    FolderName = $"{folderName}.xml",
                    Metadata = new List<Model.Metadata>()
                };
                Package.Metdata.SubFolders.Add(metadataFolder);
            }

            for (var i = 0; i < files.Count(); i++) {
                var filePath = files.ToArray()[i];
                folder.Files.Add(new Model.DocumentFile() {
                    FileName = GetValidFileName(filePath),
                    FileData = File.ReadAllBytes(filePath)
                });

                if (metadata.IsNotNull()) {
                    var fileMetadata = metadata.ToArray()[i];
                    metadataFolder.Metadata.Add(new Model.Metadata() {
                        Document = fileMetadata,
                        FileName = $"{GetValidFileName(filePath)}.xml"
                    });
                }
            }

            Package.Documents.SubFolders.Add(folder);
        }

        private void InitializePackage() {
            Package = new Model.Package() {
                Documents = new Model.DocumentSubFolder() {
                    FolderName = MainDirectoriesName.Files.GetXmlEnum(),
                    Files = new List<Model.DocumentFile>(),
                    SubFolders = new List<Model.DocumentSubFolder>()
                },
                Metdata = new Model.MetdataSubFolder() {
                    FolderName = MainDirectoriesName.Metadata.GetXmlEnum(),
                    Metadata = new List<Model.Metadata>(),
                    SubFolders = new List<Model.MetdataSubFolder>()
                },
                Objects = new Model.MetdataSubFolder() {
                    FolderName = MainDirectoriesName.Objects.GetXmlEnum(),
                    Metadata = new List<Model.Metadata>(),
                    SubFolders = new List<Model.MetdataSubFolder>()
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
    }
}
