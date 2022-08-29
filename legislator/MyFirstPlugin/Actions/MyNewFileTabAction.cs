using EAP.Editor.Plugins.Core;
using System.Drawing;
using System.Windows.Forms;

namespace MyFirstPlugin.Actions {
    public class MyNewFileTabAction : IPluginLegislatorAction {
        public ActionButtonLocation ButtonLocation => ActionButtonLocation.MenuFile_New;
        public int ButtonOrder => 0;
        public string Caption => "New external system document";
        public string Description => "New external system document description";
        public bool Enabled => true;
        public Bitmap Icon16 => Properties.Resources.MyFirstPluginFile32;
        public Bitmap Icon32 => Properties.Resources.MyFirstPluginFile32;
        public Bitmap Icon48 => Properties.Resources.MyFirstPluginFile32;
        public string MoreInfoUrl => "http://itorg.pl/";
        public bool Visible => true;

        public object Execute() {
            //if (!Plugin.ThisPlugin.UsePlugin) { throw new Exception("Add-on features are not included!"); }
            MessageBox.Show(Description, Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return this;
        }

        public void Dispose() { }
    }
}
