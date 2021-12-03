using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Abc.Nes.ArchivalPackage.Formats.Tar {
    public class TarFile : IDisposable {
        private Stream FileStream;
        private string FilePath;
        private TarType Type;

        private TarFile() { }
        public TarFile(Stream fileStream, TarType type) : this() {
            if (fileStream.IsNull() || fileStream.Length == 0) { throw new ArgumentNullException("fileStream"); }
            if (type == TarType.None) { throw new ArgumentNullException("type"); }

            FileStream = fileStream;
            Type = type;

        }
        public TarFile(string filePath, TarType type = TarType.None) : this() {
            if (filePath.IsNullOrEmpty() || !File.Exists(filePath)) { throw new FileNotFoundException(); }

            FilePath = filePath;
            Type = type;

            if (type == TarType.None) {
                if (filePath.ToLower().EndsWith(".tar.gz")) {
                    Type = TarType.TarGz;
                }
                else if (filePath.ToLower().EndsWith(".tar")) {
                    Type = TarType.Tar;
                }
            }
        }

        public void Extract(string outputDir) {
            if (!Directory.Exists(outputDir)) { Directory.CreateDirectory(outputDir); }

            if (Type == TarType.TarGz) {
                ExtractTarGz(outputDir);
            }
            if (Type == TarType.Tar) {
                ExtractTar(outputDir);
            }
        }
        public Stream ConvertToZip() {
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Extract(outputDir);
            var stream = DirectoryToZip(outputDir);
            if (stream.IsNotNull() && stream.Length > 0) {
                try {
                    Directory.Delete(outputDir, true);
                }
                catch { }
                return stream;
            }
            return default;
        }
        public bool ConvertToZip(string zipFilePath) {
            var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Extract(outputDir);
            DirectoryToZip(outputDir, zipFilePath);
            try {
                Directory.Delete(outputDir, true);
            }
            catch { }
            return File.Exists(zipFilePath);
        }

        private void ExtractTarGz(string outputDir) {
            if (FileStream.IsNotNull()) {
                ExtractTarGz(FileStream, outputDir);
            }
            else if (FilePath.IsNotNullOrEmpty() && File.Exists(FilePath)) {
                ExtractTarGz(FilePath, outputDir);
            }
        }
        private void ExtractTarGz(string filename, string outputDir) {
            using (var stream = File.OpenRead(filename))
                ExtractTarGz(stream, outputDir);
        }
        private void ExtractTarGz(Stream stream, string outputDir) {
            // A GZipStream is not seekable, so copy it first to a MemoryStream
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress)) {
                const int chunk = 4096;
                using (var memStr = new MemoryStream()) {
                    int read;
                    var buffer = new byte[chunk];
                    do {
                        read = gzip.Read(buffer, 0, chunk);
                        memStr.Write(buffer, 0, read);
                    } while (read == chunk);

                    memStr.Seek(0, SeekOrigin.Begin);
                    ExtractTar(memStr, outputDir);
                }
            }
        }

        private void ExtractTar(string outputDir) {
            if (FileStream.IsNotNull()) {
                ExtractTar(FileStream, outputDir);
            }
            else if (FilePath.IsNotNullOrEmpty() && File.Exists(FilePath)) {
                ExtractTar(FilePath, outputDir);
            }
        }
        private void ExtractTar(string filename, string outputDir) {
            using (var stream = File.OpenRead(filename))
                ExtractTar(stream, outputDir);
        }
        private void ExtractTar(Stream stream, string outputDir) {
            var buffer = new byte[100];
            while (true) {
                stream.Read(buffer, 0, 100);
                var name = Encoding.ASCII.GetString(buffer).Trim('\0');
                if (String.IsNullOrWhiteSpace(name))
                    break;
                stream.Seek(24, SeekOrigin.Current);
                stream.Read(buffer, 0, 12);
                var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                stream.Seek(376L, SeekOrigin.Current);


                var output = Path.Combine(outputDir, name.Replace("/", "\\"));

                if (!Directory.Exists(Path.GetDirectoryName(output)))
                    Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!name.Equals("./", StringComparison.InvariantCulture) && !name.EndsWith("/") && size > 0) {
                    using (var str = File.Open(output, FileMode.OpenOrCreate, FileAccess.Write)) {
                        var buf = new byte[size];
                        stream.Read(buf, 0, buf.Length);
                        str.Write(buf, 0, buf.Length);
                    }
                }

                var pos = stream.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                stream.Seek(offset, SeekOrigin.Current);
            }
        }

        private void DirectoryToZip(string dir, string zipFilePath) {
            if (File.Exists(zipFilePath)) {
                File.Delete(zipFilePath);
            }
            using (var zip = new Ionic.Zip.ZipFile(Encoding.UTF8) { }) {
                zip.UseZip64WhenSaving = Ionic.Zip.Zip64Option.AsNecessary;
                DirectoryToZip(dir, dir, zip);
                zip.Save(zipFilePath);
            }
        }

        private Stream DirectoryToZip(string dir) {
            var stream = new MemoryStream();
            using (var zip = new Ionic.Zip.ZipFile(Encoding.UTF8) { }) {
                zip.UseZip64WhenSaving = Ionic.Zip.Zip64Option.AsNecessary;
                DirectoryToZip(dir, dir, zip);
                zip.Save(stream);
            }
            return stream;
        }


        private void DirectoryToZip(string root, string dir, Ionic.Zip.ZipFile zip) {
            foreach (var item in Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly)) {
                var internalName = item.Replace(root, "").Replace("\\", "/");
                if (internalName.StartsWith("/")) { internalName = internalName.Substring(1); }
                zip.AddEntry(internalName, File.ReadAllBytes(item));
            }
            foreach (var item in Directory.GetDirectories(dir)) {
                DirectoryToZip(root, item, zip);
            }
        }

        public void Dispose() { }
    }
    public enum TarType { None, Tar, TarGz }
}
