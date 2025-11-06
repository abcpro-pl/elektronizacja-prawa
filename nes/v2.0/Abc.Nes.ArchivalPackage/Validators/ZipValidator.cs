using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;

namespace Abc.Nes.ArchivalPackage.Validators {
    public class ZipValidator {
        public static bool IsValidArchive(string filePath) {
            if (File.Exists(filePath)) {
                var fileBytes = File.ReadAllBytes(filePath);
                var fileName = Path.GetFileName(filePath);
                return IsValidArchive(fileBytes, fileName);
            }
            return false;
        }

        public static bool IsValidArchive(Stream stream, string fileName) {
            if (stream == null || !stream.CanRead) {
                return false;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // Read stream into byte array
                byte[] fileBytes;
                if (stream is MemoryStream ms) {
                    fileBytes = ms.ToArray();
                }
                else {
                    using (var memoryStream = new MemoryStream()) {
                        stream.CopyTo(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                return IsValidArchive(fileBytes, fileName);
            }
            finally {
                try {
                    stream.Position = originalPosition;
                }
                catch {
                    // Stream might not be seekable
                }
            }
        }
        public static bool IsValidArchive(byte[] data, string fileName) {
            if (data == null || data.Length < 6) {
                return false;
            }

            var extension = Path.GetExtension(fileName).ToLower();

            bool isValid = false;

            using (var stream = new MemoryStream(data)) {
                switch (extension) {
                    case ".7z":
                        isValid = Is7ZipFile(stream);
                        break;
                    case ".zip":
                        isValid = IsValidZipStream(stream);
                        break;
                    case ".tar":
                        isValid = IsTarFile(stream);
                        break;
                    case ".tgz":
                    case ".gz":
                        isValid = IsTarGzFile(stream);
                        break;
                    //case ".rar":
                    //    return IsRarFile(stream);
                    default:
                        // Try to auto-detect
                        var type = DetectArchiveType(stream);
                        isValid = type != ArchiveType.Unknown;
                        return isValid;
                        break;
                }

                if (!isValid) {
                    var type = DetectArchiveType(stream);
                    isValid = type != ArchiveType.Unknown;
                }
            }
            return isValid;
        }
        public static bool IsValidZipFile(string filePath, bool testExtract = false) {
#if NET48
            return Ionic.Zip.ZipFile.IsZipFile(filePath, testExtract);
#else
            try {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false)) {
                        // If no exception is thrown, it's a valid ZIP file.
                        return true;
                    }
                }
            }
            catch (InvalidDataException) {
                // The file is not a valid ZIP archive.
                return false;
            }
            catch (Exception) {
                // Handle any other unexpected errors, such as file not found.
                return false;
            }
#endif
        }

        public static bool IsValidZipStream(Stream stream, bool testExtract = false) {
#if NET48
            return Ionic.Zip.ZipFile.IsZipFile(stream, testExtract);
#else
            try {
                // Ensure that the stream is at the beginning
                if (stream.Position != 0) {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                // Try to read the stream as a ZIP archive
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true)) {
                    // If no exception is thrown, it's a valid ZIP stream
                    return true;
                }
            }
            catch (InvalidDataException) {
                // The stream is not a valid ZIP archive
                return false;
            }
            catch (Exception) {
                // Handle any other unexpected errors
                return false;
            }
#endif
        }

        public static bool IsValidTarFile(string filePath, bool testExtract = false) {
            // Open the file stream
            try {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    return IsValidTarStream(fs, testExtract);
                }
            }
            catch (Exception ex) {
                return false;
            }
        }

        public static bool IsValidTarStream(Stream stream, bool testExtract = false) {
            // Open the file stream
            try {
                // Check if the file is at least 257 bytes long (size of a header)
                if (stream.Length >= 257) {
                    byte[] buffer = new byte[257];
                    stream.Read(buffer, 0, 257);

                    // TAR file header magic typically starts at byte 257 and should be "ustar"
                    string magic = System.Text.Encoding.ASCII.GetString(buffer, 257, 5);
                    if (magic == "ustar") {
                        //Console.WriteLine("The file is a TAR file.");
                        return true;
                    }
                    else {
                        //Console.WriteLine("The file is not a TAR file.");
                        return false;
                    }
                }
                else {
                    //Console.WriteLine("The file is too small to be a TAR file.");
                    return false;
                }
            } catch(Exception ex) {
                return false;
            }
        }

        #region 7zip
        public static bool Is7ZipFile(byte[] data) {
            if (data == null || data.Length < 32) {
                return false; // 7z signature header is 32 bytes
            }

            using (var stream = new MemoryStream(data)) {
                return Is7ZipFile(stream);
            }
        }

        public static bool Is7ZipFile(Stream stream) {
            if (stream == null || !stream.CanRead || !stream.CanSeek) {
                return false;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // Check 7z signature (magic bytes)
                if (!Has7ZipSignature(stream)) {
                    return false;
                }

                // Reset position
                stream.Position = 0;

                // Try to open with SharpCompress to validate structure
                try {
                    using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(stream, new SharpCompress.Readers.ReaderOptions {
                        LeaveStreamOpen = true
                    })) {
                        // Successfully opened - valid 7z file
                        // Optionally, try to enumerate entries to ensure it's not corrupted
                        var entryCount = archive.Entries.Count();
                        return true;
                    }
                }
                catch (SharpCompress.Common.InvalidFormatException) {
                    return false;
                }
                catch (SharpCompress.Common.ArchiveException) {
                    return false;
                }
                catch (Exception) {
                    return false;
                }
            }
            finally {
                try {
                    stream.Position = originalPosition;
                }
                catch {
                    // Stream might not be seekable after operations
                }
            }
        }

        public static bool Has7ZipSignature(Stream stream) {
            if (stream == null || stream.Length < 6) {
                return false;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // 7z signature: "7z¼¯'" (0x37 0x7A 0xBC 0xAF 0x27 0x1C)
                byte[] signature = new byte[6];
                int bytesRead = stream.Read(signature, 0, 6);

                if (bytesRead < 6) {
                    return false;
                }

                return signature[0] == 0x37 &&
                       signature[1] == 0x7A &&
                       signature[2] == 0xBC &&
                       signature[3] == 0xAF &&
                       signature[4] == 0x27 &&
                       signature[5] == 0x1C;
            }
            finally {
                stream.Position = originalPosition;
            }
        }

        // Advanced version with detailed validation
        public static (bool isValid, string errorMessage) Validate7ZipFile(Stream stream) {
            if (stream == null) {
                return (false, "Stream is null");
            }

            if (!stream.CanRead) {
                return (false, "Stream is not readable");
            }

            if (!stream.CanSeek) {
                return (false, "Stream is not seekable");
            }

            if (stream.Length < 32) {
                return (false, "File is too small to be a valid 7z archive");
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // Check signature
                if (!Has7ZipSignature(stream)) {
                    return (false, "Invalid 7z signature");
                }

                stream.Position = 0;

                // Try to open and validate
                try {
                    using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(stream, new SharpCompress.Readers.ReaderOptions {
                        LeaveStreamOpen = true
                    })) {
                        // Check if we can enumerate entries
                        try {
                            int entryCount = 0;
                            foreach (var entry in archive.Entries) {
                                entryCount++;
                                // Validate entry has required properties
                                if (string.IsNullOrEmpty(entry.Key)) {
                                    return (false, "Archive contains entry with invalid key");
                                }
                            }

                            return (true, $"Valid 7z archive with {entryCount} entries");
                        }
                        catch (Exception ex) {
                            return (false, $"Error reading archive entries: {ex.Message}");
                        }
                    }
                }
                catch (SharpCompress.Common.CryptographicException ex) {
                    return (false, $"Archive is password protected or encrypted: {ex.Message}");
                }
                catch (SharpCompress.Common.InvalidFormatException ex) {
                    return (false, $"Invalid 7z format: {ex.Message}");
                }
                catch (SharpCompress.Common.ArchiveException ex) {
                    return (false, $"Archive error: {ex.Message}");
                }
                catch (Exception ex) {
                    return (false, $"Unexpected error: {ex.Message}");
                }
            }
            finally {
                try {
                    stream.Position = originalPosition;
                }
                catch {
                    // Ignore if stream is no longer seekable
                }
            }
        }

        // Quick signature check only (fast, less thorough)
        public static bool Is7ZipFileQuick(Stream stream) {
            return Has7ZipSignature(stream);
        }

        public static bool Is7ZipFileQuick(byte[] data) {
            if (data == null || data.Length < 6) {
                return false;
            }

            using (var stream = new MemoryStream(data)) {
                return Has7ZipSignature(stream);
            }
        }
        #endregion

        #region tar
        public static bool IsTarFile(Stream stream) {
            if (stream == null || stream.Length < 512) {
                return false; // TAR header is 512 bytes
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // Read TAR header (512 bytes)
                byte[] header = new byte[512];
                int bytesRead = stream.Read(header, 0, 512);

                if (bytesRead < 512) {
                    return false;
                }

                // Check if all bytes are zero (empty/invalid)
                if (header.All(b => b == 0)) {
                    return false;
                }

                // Verify TAR magic value at offset 257
                // Standard TAR: "ustar\0" (ustar followed by null)
                // GNU TAR: "ustar  \0" (ustar followed by two spaces and null)
                string magic = System.Text.Encoding.ASCII.GetString(header, 257, 6);
                if (magic != "ustar\0" && magic != "ustar ") {
                    return false;
                }

                // Verify checksum (optional but recommended)
                return VerifyTarChecksum(header);
            }
            catch {
                return false;
            }
            finally {
                stream.Position = originalPosition;
            }
        }

        private static bool VerifyTarChecksum(byte[] header) {
            try {
                // Checksum is stored at offset 148-155 (8 bytes, octal, space or null terminated)
                string checksumString = System.Text.Encoding.ASCII.GetString(header, 148, 8).Trim('\0', ' ');

                if (string.IsNullOrWhiteSpace(checksumString)) {
                    return false;
                }

                // Parse the stored checksum (octal format)
                int storedChecksum = Convert.ToInt32(checksumString, 8);

                // Calculate checksum: sum of all bytes, treating checksum field as spaces
                int calculatedChecksum = 0;
                for (int i = 0; i < 512; i++) {
                    if (i >= 148 && i < 156) {
                        // Treat checksum field as spaces
                        calculatedChecksum += 32; // ASCII space
                    }
                    else {
                        calculatedChecksum += header[i];
                    }
                }

                return storedChecksum == calculatedChecksum;
            }
            catch {
                return false;
            }
        }
        #endregion

        #region tarGz
        public static bool IsTarGzFile(byte[] data) {
            if (data == null || data.Length < 10) {
                return false;
            }

            using (var stream = new MemoryStream(data)) {
                return IsTarGzFile(stream);
            }
        }

        public static bool IsTarGzFile(Stream stream) {
            if (stream == null || !stream.CanRead || !stream.CanSeek) {
                return false;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // Check GZIP magic number (1F 8B)
                if (!IsGZipFile(stream)) {
                    return false;
                }

                // Reset to beginning
                stream.Position = 0;

                // Try to decompress and check if it's a valid TAR
                try {
                    using (var gzipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
                    using (var decompressed = new MemoryStream()) {
                        gzipStream.CopyTo(decompressed);
                        decompressed.Position = 0;

                        // Check if decompressed content is a valid TAR
                        return IsTarFile(decompressed);
                    }
                }
                catch {
                    return false;
                }
            }
            finally {
                // Restore original position
                try {
                    stream.Position = originalPosition;
                }
                catch {
                    // Stream might not be seekable after operations
                }
            }
        }

        public static bool IsGZipFile(Stream stream) {
            if (stream == null || stream.Length < 2) {
                return false;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // GZIP magic number: 1F 8B
                byte[] magic = new byte[2];
                int bytesRead = stream.Read(magic, 0, 2);

                return bytesRead == 2 && magic[0] == 0x1F && magic[1] == 0x8B;
            }
            finally {
                stream.Position = originalPosition;
            }
        }

        public static bool IsTarGzFileUsingSharpCompress(Stream stream) {
            if (stream == null || !stream.CanRead || !stream.CanSeek) {
                return false;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;

                // Check GZIP magic number first
                if (!IsGZipFile(stream)) {
                    return false;
                }

                stream.Position = 0;

                // Try to open as TAR.GZ using SharpCompress
                using (var gzipStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
                using (var reader = SharpCompress.Readers.Tar.TarReader.Open(gzipStream)) {
                    // Try to read first entry
                    if (reader.MoveToNextEntry()) {
                        return true;
                    }
                    return false; // Empty TAR
                }
            }
            catch (SharpCompress.Common.InvalidFormatException) {
                return false;
            }
            catch (InvalidDataException) {
                return false;
            }
            catch (Exception) {
                return false;
            }
            finally {
                try {
                    stream.Position = originalPosition;
                }
                catch {
                    // Ignore if stream is no longer seekable
                }
            }
        }

        public static bool IsTarGzFileUsingSharpCompress(byte[] data) {
            if (data == null || data.Length < 10) {
                return false;
            }

            using (var stream = new MemoryStream(data)) {
                return IsTarGzFileUsingSharpCompress(stream);
            }
        }
        #endregion

        #region detect archive
        public enum ArchiveType {
            Unknown,
            Zip,
            SevenZip,
            Tar,
            TarGz,
            GZip,
            Rar
        }

        public static ArchiveType DetectArchiveType(Stream stream) {
            if (stream == null || !stream.CanRead || !stream.CanSeek || stream.Length < 6) {
                return ArchiveType.Unknown;
            }

            long originalPosition = stream.Position;

            try {
                stream.Position = 0;
                byte[] signature = new byte[6];
                int bytesRead = stream.Read(signature, 0, 6);

                if (bytesRead < 6) {
                    return ArchiveType.Unknown;
                }

                // Check 7z signature: 37 7A BC AF 27 1C
                if (signature[0] == 0x37 && signature[1] == 0x7A &&
                    signature[2] == 0xBC && signature[3] == 0xAF &&
                    signature[4] == 0x27 && signature[5] == 0x1C) {
                    return ArchiveType.SevenZip;
                }

                // Check ZIP signature: 50 4B (PK)
                if (signature[0] == 0x50 && signature[1] == 0x4B) {
                    return ArchiveType.Zip;
                }

                // Check GZIP signature: 1F 8B
                if (signature[0] == 0x1F && signature[1] == 0x8B) {
                    // Could be .gz or .tar.gz
                    stream.Position = 0;
                    if (IsTarGzFile(stream)) {
                        return ArchiveType.TarGz;
                    }
                    return ArchiveType.GZip;
                }

                // Check RAR signature: 52 61 72 21 (Rar!)
                if (signature[0] == 0x52 && signature[1] == 0x61 &&
                    signature[2] == 0x72 && signature[3] == 0x21) {
                    return ArchiveType.Rar;
                }

                // Check TAR (no magic bytes at start, check at offset 257)
                if (stream.Length >= 512) {
                    stream.Position = 0;
                    if (IsTarFile(stream)) {
                        return ArchiveType.Tar;
                    }
                }

                return ArchiveType.Unknown;
            }
            finally {
                stream.Position = originalPosition;
            }
        }

        public static ArchiveType DetectArchiveType(byte[] data) {
            using (var stream = new MemoryStream(data)) {
                return DetectArchiveType(stream);
            }
        }
        #endregion
    }
}