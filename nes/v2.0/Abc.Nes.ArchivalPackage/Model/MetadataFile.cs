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
using System.Text;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Model {
    public class MetadataFile : DocumentFile {
        public IDocument Document { get; set; }
        public string DocumentFileName { get { return DocumentFilePath.IsNotNullOrEmpty() ? Path.GetFileName(DocumentFilePath) : null; } }
        public string DocumentFilePath { get; set; }
        public override void Init(byte[] fileData, out Exception ex) {
            ex = null;
            FileData = fileData;
            try {
                Document = new Converters.XmlConverter().ParseXml(TrimBom(fileData));
            }
            catch (Exception e) {
                ex = e;
            }
        }

        private string TrimBom(byte[] data) {
            bool bom = (data[0] == 239 && data[1] == 187 && data[2] == 191); // Detect BOM, 239, 187, 191
            int substract = bom ? 3 : 0;
            string feedStr = Encoding.UTF8.GetString(data, bom ? substract : 0, data.Length - substract);
            // do something
            var e = XElement.Parse(feedStr);
            return e.ToString();
        }
    }

    public class MetadataFolder : Folder<MetadataFolder, MetadataFile> {
        public override List<MetadataFile> Items { get; set; }
        public override List<MetadataFolder> Folders { get; set; }

        public override FolderBase CreateSubFolder(string folderName) {
            if (folderName.IsNullOrEmpty()) { throw new ArgumentNullException(); }
            var folder = new MetadataFolder() {
                FolderName = folderName,
                Folders = new List<MetadataFolder>(),
                Items = new List<MetadataFile>()
            };
            if (Folders.IsNull()) { Folders = new List<MetadataFolder>(); }
            Folders.Add(folder);
            return folder;
        }
    }
}
