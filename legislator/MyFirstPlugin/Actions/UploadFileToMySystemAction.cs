/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using EAP.Editor.Plugins.Core;
using MyFirstPlugin.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyFirstPlugin.Actions {
    public class UploadFileToMySystemAction : IPluginLegislatorAction, IDisposable {
        public ActionButtonLocation ButtonLocation => ActionButtonLocation.EditorForm; 
        public int ButtonOrder => 0; 
        public string Caption => "Send to system"; 
        public string Description => "Save the document in the system document repository"; 
        public bool Enabled => true; 
        public Bitmap Icon16 => Properties.Resources.Save32; 
        public Bitmap Icon32 => Properties.Resources.Save32; 
        public Bitmap Icon48 => Properties.Resources.Save32; 
        public string MoreInfoUrl => "https://itorg.pl/"; 
        public bool Visible => true; 
     
        public object Execute() {
            if (!Plugin.ThisPlugin.UsePlugin) { throw new Exception("Add-on features are not included!"); }
            Plugin.ThisPlugin.ControllerInstance.SaveCurrentDocument();
            var filePath = Plugin.ThisPlugin.ControllerInstance.GetCurrentDocumentPath();

            Plugin.ThisPlugin.ApiController.ShowWaitForm("Send to system...");
            var isBusy = true;
            var task = Plugin.ThisPlugin.SendDocument(filePath);
            task.ContinueWith(t => {
                Plugin.ThisPlugin.ApiController.HideWaitForm();
                isBusy = false;

                if (t.Result.Success) {
                    Plugin.ThisPlugin.ApiController.ShowInformation(t.Result.Message);
                }
                else if (t.Result.Message.IsNotNullOrEmpty()) {
                    Plugin.ThisPlugin.ApiController.ShowWarning(t.Result.Message);
                }
            });

            while (isBusy) {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
            }


            return this;
        }

        public void Dispose() { }
    }
}
