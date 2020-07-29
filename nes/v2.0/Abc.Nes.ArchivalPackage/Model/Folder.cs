/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Collections.Generic;

namespace Abc.Nes.ArchivalPackage.Model {
    public abstract class Folder<F, T> : FolderBase
        where F : FolderBase
        where T : ItemBase {
        public abstract List<T> Items { get; set; }
        public abstract List<F> Folders { get; set; }
        public override FolderBase GetFolder(string folderName) {
            if (Folders.IsNotNull()) {
                foreach (FolderBase folder in Folders) {
                    if (folder.FolderName == folderName) {
                        return folder;
                    }
                }
            }

            return default;
        }
        public override ItemBase AddItem(ItemBase item) {
            if (Items.IsNull()) { Items = new List<T>(); }
            Items.Add(item as T);
            return item;
        }
        public bool IsEmpty {
            get {
                var itemsIsEmpty = Items.IsNull() || Items.Count == 0;
                var foldersIsEmpty = Folders.IsNull() || Folders.Count == 0;
                return itemsIsEmpty && foldersIsEmpty;
            }
        }

        public override IEnumerable<FolderBase> GetFolders() => Folders;
        public override IEnumerable<ItemBase> GetItems() => Items;
    }
}
