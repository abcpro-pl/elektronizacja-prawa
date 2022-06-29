﻿/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;
using System.Drawing;
using EAP.Editor.Plugins.Core;

namespace MyFirstPlugin.Actions {
    public class OpenFromSystemAction : IPluginLegislatorAction {
        public ActionButtonLocation ButtonLocation { get { return ActionButtonLocation.MenuFile_Root; } }
        public int ButtonOrder { get { return 1; } }
        public string Caption { get { return "System EZD"; } }
        public string Description { get { return "Dowiedz się więcej o systemie"; } }
        public bool Enabled { get { return true; } }
        public Bitmap Icon16 { get { return Properties.Resources.Plugin32; } }
        public Bitmap Icon32 { get { return Properties.Resources.Plugin32; } }
        public Bitmap Icon48 { get { return Properties.Resources.Plugin48; } }
        public string MoreInfoUrl { get { return "http://itorg.pl/"; } }
        public bool Visible { get { return true; } }
        public void Dispose() { }

        public object Execute() {
            //if (!Plugin.ThisPlugin.UsePlugin) { throw new Exception("Funkcje dodatku nie są włączone!"); }
            System.Diagnostics.Process.Start("http://itorg.pl/");
            return this;
        }
    }
}
