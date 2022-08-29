/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using EAP.Editor.Plugins.Core;
using MyFirstPlugin.Model;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MyFirstPlugin.Actions {
    public class OpenTabItemsAction : IPluginLegislatorAction, IPluginLegislatorActionItems, IDisposable {
        public string Caption => "";
        public string Description => "";
        public Bitmap Icon16 => null;
        public Bitmap Icon32 => null;
        public Bitmap Icon48 => null;
        public ActionButtonLocation ButtonLocation => ActionButtonLocation.MenuFile_Open;
        public int ButtonOrder => 0;
        public bool Visible => true;
        public bool Enabled => true;
        public string MoreInfoUrl => "http://itorg.pl/";

        public void Dispose() { }

        public object Execute() => null;

        public IEnumerable<IPluginLegislatorActionItem> GetItems() {
            var list =new List<IPluginLegislatorActionItem>();
            for (int i = 0; i < 15; i++) {
                list.Add(new RecentlyUsedItem() { Caption = $"Name of file {i}", Description = $"Description of file {i}", Icon32 = Properties.Resources.MyFirstPluginFile32, Tag = $"File action {i} data" });
            }
            return list;
        }
    }   
}
