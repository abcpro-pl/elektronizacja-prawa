using EAP.Editor.Plugins.Core;
using System.Drawing;

namespace MyFirstPlugin.Model {
    public class RecentlyUsedItem : IPluginLegislatorActionItem {
        public Bitmap Icon32 { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public object Tag { get; set; }

        public object Execute() {
            var message = "Hello world!";
            if (Tag != null) {
                message = Tag.ToString();
            }

            return System.Windows.Forms.MessageBox.Show(message, Caption, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }
    }
}
