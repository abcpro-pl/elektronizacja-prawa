/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;
using System.Windows.Forms;

namespace MyFirstPlugin.Controls {
    public partial class PluginOptionsControl : UserControl {
        public PluginOptionsControl() {
            InitializeComponent();
            LoadData();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            if (this.ParentForm != null) {
                this.ParentForm.FormClosing += new FormClosingEventHandler(delegate (object sender, FormClosingEventArgs arg) {
                    Plugin.ThisPlugin.SaveConfiguration();
                });
            }
        }

        private void LoadData() {
            cbEnabled.Checked = Plugin.ThisPlugin.UsePlugin;
        }

        private void cbEnabled_CheckedChanged(object sender, EventArgs e) {
            if (Plugin.ThisPlugin.Configuration.UsePlugin != cbEnabled.Checked) {
                Plugin.ThisPlugin.Configuration.UsePlugin = cbEnabled.Checked;               
            }
        }
    }
}
