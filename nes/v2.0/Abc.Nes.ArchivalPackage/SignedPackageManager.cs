/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

//using Ionic.Zip;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Abc.Nes.ArchivalPackage {
    public class SignedPackageManager : ISignedPackageManager {
        public SignedPackageInfo Extract(string filePath, string outputDir = null) {
            if (File.Exists(filePath)) {
                if (outputDir.IsNullOrEmpty()) {
                    outputDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath));

                    if (!Directory.Exists(outputDir)) {
                        Directory.CreateDirectory(outputDir);
                    }
                }

                if (Directory.Exists(outputDir)) {
                    foreach (var item in Directory.GetFiles(outputDir)) {
                        File.Delete(item);
                    }
                }

                var result = new SignedPackageInfo() {
                    FileName = filePath,
                    Directory = outputDir
                };
#if NET48
                using (var zipFile = ZipFile.Read(result.FileName)) {
                    if (zipFile.EntryFileNames.Where(x => x.ToLower().Contains("dokumenty")).Any()) {
                        // to jest paczka eADM
                        result.Directory = Path.GetDirectoryName(filePath);
                        result.PackageFileName = Path.GetFileName(filePath);
                        return result;
                    }
                    else {
                        zipFile.ExtractAll(result.Directory);
                    }
                }
#else
                using (var zipStream = new FileStream(result.FileName, FileMode.Open, FileAccess.Read))
                using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Read, true)) {
                    if (zipFile.Entries.Where(x => x.Name.ToLower().Contains("dokumenty")).Any()) {
                        result.Directory = Path.GetDirectoryName(filePath);
                        result.PackageFileName = Path.GetFileName(filePath);
                        return result;
                    } else {
                        zipFile.ExtractToDirectory(result.Directory);
                    }
                }
#endif

                foreach (var file in Directory.GetFiles(result.Directory)) {
                    if (Path.GetExtension(file) == ".xades") {
                        result.SignatureFileName = Path.GetFileName(file);
                    }
                    else if (Path.GetExtension(file) == ".zip" || Path.GetExtension(file) == ".tar" || Path.GetExtension(file) == ".gz") {
                        result.PackageFileName = Path.GetFileName(file);
                    }
                }

                if (result.SignatureFileName.IsNotNullOrEmpty() && result.PackageFileName.IsNotNullOrEmpty()) {
                    return result;
                }
            }
            return default;
        }

        public void Compress(SignedPackageInfo info) {
#if NET48
            using (var zipFile = new ZipFile(info.FileName)) {
                zipFile.AddFile(Path.Combine(info.Directory, info.PackageFileName));
                zipFile.AddFile(Path.Combine(info.Directory, info.SignatureFileName));
                zipFile.Save();
            }
#else
            using (var zipFileStream = new FileStream(info.FileName, FileMode.Create))
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create)) {
                zipArchive.CreateEntryFromFile(Path.Combine(info.Directory, info.PackageFileName), info.PackageFileName);
                zipArchive.CreateEntryFromFile(Path.Combine(info.Directory, info.SignatureFileName), info.SignatureFileName);
            }
#endif
        }

        public void Dispose() { }
    }


    public class SignedPackageInfo {
        public string FileName { get; set; }
        public string Directory { get; set; }
        public string PackageFileName { get; set; }
        public string SignatureFileName { get; set; }
    }
}
