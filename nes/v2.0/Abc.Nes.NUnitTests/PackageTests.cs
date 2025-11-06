using Abc.Nes.ArchivalPackage;
using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Converters;
using Aspose.Zip.Tar;
using ICSharpCode.SharpZipLib.Tar;
using NUnit.Framework;
using SharpCompress.Archives;
using SharpCompress.Readers;
using System;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Abc.Nes.NUnitTests {
    public class PackageTests {
        string pwd;
        string testFilesDirPath;
        string archiveFilePath;

        [SetUp]
        public void Setup() {

            pwd = Directory.GetCurrentDirectory();
            testFilesDirPath = Path.GetFullPath(Path.Combine(pwd, @"../../../../sample/"));
            //wsl
            archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            //windows
            archiveFilePath = Path.Combine("d:\\", "Paczki", "paczka0.tar");
        }

        [Test]
        public void ValidateXadesSignature() {
            var pathToPackage = Path.Combine(testFilesDirPath, "paczka0real.zip");
            var pathToFile = "dokumenty/17264112021Wy/17264112021Wy_1.xml";

            var packageManager = new PackageManager();
            packageManager.LoadPackage(pathToPackage, out _);

            var documentFile = packageManager.Package.GetItemByFilePath(pathToFile) as DocumentFile;



        }

        [Test]
        public void ParseXml() {
            
            
            var filePath = Path.Combine(testFilesDirPath, "testXml.xml");
            var conv = new XmlConverter();
            var xmlText = File.ReadAllText(filePath);
            IDocument doc = null;
            try {

                doc = conv.ParseXml(xmlText);
            }
            catch (Exception ex) {

            }
            Assert.IsNotNull(doc);
        }

        [Test]
        public void Package16WithZipInEmail() {
            var filePath = Path.Combine(testFilesDirPath, "SM_paczka_email_z_zip.ZIP");
            using (var packageManager = new PackageManager()) {
                packageManager.LoadPackage(filePath, out _);

                var result = packageManager.GetValidationResult(true, true);

                Assert.IsNotNull(result.IsCorrect);
            }
        }

        [Test]
        public void HandleTarGzPackageFile() {
            var filePath = Path.Combine(testFilesDirPath, "eADM-package-26324.tar.gz");
            using (var packageManager = new PackageManager()) {
                packageManager.LoadPackage(filePath, out _);
                

                var result = packageManager.GetValidationResult(true, true);

                Assert.IsNotNull(result.IsCorrect);
            }
        }

        [Test]
        public void HandleTarGzPackageStream() {
            var filePath = Path.Combine(testFilesDirPath, "eADM-package-26324.tar.gz");
            using (var packageManager = new PackageManager()) {
                using (var fileStream = File.OpenRead(filePath)) {
                    packageManager.LoadPackage(fileStream, out _);
                }

                var result = packageManager.GetValidationResult(true, true);

                Assert.IsNotNull(result.IsCorrect);
            }
        }

        [Test]
        public void HandleTarPackageFile() {
            var filePath = Path.Combine(testFilesDirPath, "eADM-package-26324.tar");
            using (var packageManager = new PackageManager()) {
                packageManager.LoadPackage(filePath, out _);

                var result = packageManager.GetValidationResult(true, true);

                Assert.IsNotNull(result.IsCorrect);
            }
        }

        [Test]
        public void HandleTarPackageStream() {
            var filePath = Path.Combine(testFilesDirPath, "eADM-package-26324.tar");
            using (var packageManager = new PackageManager()) {
                using (var fileStream = File.OpenRead(filePath)) {
                    packageManager.LoadPackage(fileStream, out _);


                    var result = packageManager.GetValidationResult(true, true);

                    Assert.IsNotNull(result.IsCorrect);
                }
            }
        }

        [Test]
        public void LoadPAckageWrongCaseIdElement() {
            var pathToPackage = Path.Combine(testFilesDirPath, "Z_00260807.zip");
            //pathToPackage = Path.Combine(testFilesDirPath, "Z_00260807 — kopia.zip");
            //pathToPackage = Path.Combine(testFilesDirPath, "dokumenty.zip");
            
            var pathToFile = "dokumenty/17264112021Wy/17264112021Wy_1.xml";

            var packageManager = new PackageManager();
            packageManager.LoadPackage(pathToPackage, out _);

            packageManager.GetValidationResult(true, false);

            var documentFile = packageManager.Package.GetItemByFilePath(pathToFile) as DocumentFile;

            var filePath = "dokumenty/8)Urz⌐dowe Poÿwiadczenie Dor⌐czenia eUS.XML";
            var x = "dokumenty/8)Urzędowe Poświadczenie Doręczenia eUS.XML";

            using (var packageSignerManager = new PackageSignerManager()) {
                //var verifiedSignaturesForDocument = packageSignerManager.VerifySignatures(pathToPackage, filePath);
                var verifiedSignaturesForDocument2 = packageSignerManager.VerifySignatures(pathToPackage, x);
            }

        }


        [Test]
        public void WeirdPackageLoad() {
            var pathToPackage = Path.Combine(testFilesDirPath, "eADM-package-35857.zip");
            pathToPackage = Path.Combine(testFilesDirPath, "mala-paczka-test.zip");

            var mgr = new PackageManager();
            Exception exc=null;
            mgr.LoadPackage(pathToPackage, out exc );

            Assert.IsTrue(exc == null );
        }

// tar extract tests
        [Test]
        public void UnpackSharpCompress() {
            //var archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            var destinationPath = archiveFilePath + "SC";
            if (Directory.Exists(destinationPath)) Directory.Delete(destinationPath, true);

            Directory.CreateDirectory(destinationPath);

            using var archive = SharpCompress.Archives.ArchiveFactory.Open(archiveFilePath);
            archive.ExtractToDirectory(destinationPath);
        }

        [Test]
        public void UnpackSharpCompressFix() {
            //var archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            var destinationPath = archiveFilePath + "SCFix";
            if (Directory.Exists(destinationPath)) Directory.Delete(destinationPath, true);

            using (var archive = SharpCompress.Archives.ArchiveFactory.Open(archiveFilePath)) {
                foreach (var entry in archive.Entries) {
                    if (!entry.IsDirectory) {
                        string fixPath = entry.Key.Replace("\\", $"{Path.DirectorySeparatorChar}");
                        string filePath = Path.Combine(destinationPath, fixPath);
                        string dirPath = Path.GetDirectoryName(filePath);

                        if (!Directory.Exists(dirPath)) {
                            Directory.CreateDirectory(dirPath);
                        }

                        entry.WriteToFile(filePath, new SharpCompress.Common.ExtractionOptions() {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }

        }

        [Test]
        public void UnpackSharpCompress2() {
            //var archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            var destinationPath = archiveFilePath + "SC2";
            if (Directory.Exists(destinationPath)) Directory.Delete(destinationPath, true);

            using (Stream stream = File.OpenRead(archiveFilePath))
            using (var reader = ReaderFactory.Open(stream)) {
                if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);
                while (reader.MoveToNextEntry()) {
                    if (!reader.Entry.IsDirectory) {
                        Console.WriteLine(reader.Entry.Key);

                        var nameFix = reader.Entry.Key.Replace("\\", $"{Path.DirectorySeparatorChar}");
                        var filePath = Path.Combine(destinationPath, nameFix);
                        var fileDirPath = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(fileDirPath)) {
                            Directory.CreateDirectory(fileDirPath);
                        }
                        reader.WriteEntryToFile(filePath, new SharpCompress.Common.ExtractionOptions {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }
        [Test]
        public void UnpackSharpZipLib() {
            //var archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            var destinationPath = archiveFilePath + "SZL";
            if (Directory.Exists(destinationPath)) Directory.Delete(destinationPath, true);


            Directory.CreateDirectory(destinationPath);

            var stream = File.OpenRead(archiveFilePath);
            ICSharpCode.SharpZipLib.Tar.TarArchive ta = ICSharpCode.SharpZipLib.Tar.TarArchive.CreateInputTarArchive(stream);
            ta.ExtractContents(destinationPath, true);
            ta.Close();
            stream.Close();
        }

        [Test]
        public void UnpackTarNative() {
            //var archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            //var archiveFilePath = Path.Combine("d:\\", "Paczki", "paczka0.tar");
            var destinationPath = archiveFilePath + "NET8";
            if (Directory.Exists(destinationPath)) Directory.Delete(destinationPath, true);

            //var testPath = Path.Combine(destinationPath, "test", "ala", "ma", "kota");
            //Directory.CreateDirectory(testPath);

            var tStream = new FileStream(archiveFilePath, FileMode.Open);
            var tReadere = new System.Formats.Tar.TarReader(tStream);
            System.Formats.Tar.TarEntry entry;
            Directory.CreateDirectory(destinationPath);
            while ((entry = tReadere.GetNextEntry()) != null) {
                if (entry.EntryType is TarEntryType.SymbolicLink or TarEntryType.HardLink or TarEntryType.GlobalExtendedAttributes) {
                    continue;
                }

                Console.WriteLine($"Extract {entry.Name}");
                var fixName = entry.Name.Replace("\\", $"{Path.DirectorySeparatorChar}");
                var filePath = Path.Join(destinationPath, fixName);
                var fileDir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(fileDir)) {
                    Directory.CreateDirectory(fileDir);
                }
                entry.ExtractToFile(filePath, true);
            }
        }

        [Test]
        public void UnpackAspose() {
            //var archiveFilePath = Path.Combine("/mnt", "d", "Paczki", "paczka0.tar");
            var destinationPath = archiveFilePath + "A";
            if (Directory.Exists(destinationPath)) Directory.Delete(destinationPath, true);

            using (var arch = new Aspose.Zip.Tar.TarArchive(archiveFilePath)) {
                arch.ExtractToDirectory(destinationPath);
            }
        }
    }
}
