/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Ionic.Zip;
using System.IO;

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
                using (var zipFile = ZipFile.Read(result.FileName)) {
                    zipFile.ExtractAll(result.Directory);
                }

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
            using (var zipFile = new ZipFile(info.FileName)) {
                zipFile.AddFile(Path.Combine(info.Directory, info.PackageFileName));
                zipFile.AddFile(Path.Combine(info.Directory, info.SignatureFileName));
                zipFile.Save();
            }
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
