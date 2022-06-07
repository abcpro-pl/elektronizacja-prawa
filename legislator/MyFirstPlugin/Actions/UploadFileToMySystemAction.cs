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
        public ActionButtonLocation ButtonLocation { get { return ActionButtonLocation.EditorForm; } }
        public int ButtonOrder { get { return 0; } }
        public string Caption { get { return "Wyślij do systemu"; } }
        public string Description { get { return "Zapisz dokument w repozytorium dokumentów systemu"; } }
        public bool Enabled { get { return true; } }
        public Bitmap Icon16 { get { return Properties.Resources.Save32; } }
        public Bitmap Icon32 { get { return Properties.Resources.Save32; } }
        public Bitmap Icon48 { get { return Properties.Resources.Save32; } }
        public string MoreInfoUrl { get { return "https://itorg.pl/"; } }
        public bool Visible { get { return true; } }
        public void Dispose() { }

        public object Execute() {
            if (!Plugin.ThisPlugin.UsePlugin) { throw new Exception("Funkcje dodatku nie są włączone!"); }
            Plugin.ThisPlugin.ControllerInstance.SaveCurrentDocument();
            var filePath = Plugin.ThisPlugin.ControllerInstance.GetCurrentDocumentPath();

            Plugin.ThisPlugin.ApiController.ShowWaitForm("Wysyłanie do systemu...");
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
    }
}
