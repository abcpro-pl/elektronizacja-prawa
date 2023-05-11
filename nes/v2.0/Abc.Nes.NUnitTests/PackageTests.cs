using Abc.Nes.ArchivalPackage;
using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.ArchivalPackage.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Abc.Nes.NUnitTests {
    public class PackageTests {
        string pwd;
        string testFilesDirPath;

        [SetUp]
        public void Setup() {

            pwd = Directory.GetCurrentDirectory();
            testFilesDirPath = Path.GetFullPath(Path.Combine(pwd, @"../../../../sample/"));
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
    }
}
