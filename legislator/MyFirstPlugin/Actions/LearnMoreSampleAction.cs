/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;
using System.Drawing;
using EAP.Editor.Plugins.Core;

namespace MyFirstPlugin.Actions {
    public class LearnMoreSampleAction : IPluginLegislatorAction, IDisposable {
        public ActionButtonLocation ButtonLocation => ActionButtonLocation.EditorForm; 
        public int ButtonOrder => 1;
        public string Caption => "Get to know our system";
        public string Description => "Find out more about this system";
        public bool Enabled => true; 
        public Bitmap Icon16 => Properties.Resources.Plugin32; 
        public Bitmap Icon32 => Properties.Resources.Plugin32; 
        public Bitmap Icon48 => Properties.Resources.Plugin48; 
        public string MoreInfoUrl => "http://itorg.pl/"; 
        public bool Visible => true; 

        public object Execute() {
            //if (!Plugin.ThisPlugin.UsePlugin) { throw new Exception("Add-on features are not included!"); }
            System.Diagnostics.Process.Start("http://itorg.pl/");
            return this;
        }

        public void Dispose() { }
    }
}
