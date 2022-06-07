/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using EAP.Editor.Plugins.Core;
using EAP.Editor.Plugins.Core.API;
using MyFirstPlugin.Controls;
using MyFirstPlugin.Extensions;
using MyFirstPlugin.Model;
using MyFirstPlugin.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyFirstPlugin {
    public class Plugin : IPluginLegislator, IPluginLegislatorEvents, IPluginLegislatorCustomBehavior {
        private static Plugin m_ThisPlugin = null;
        public static Plugin ThisPlugin => m_ThisPlugin;

        private ILegislatorApiController apiController;
        private PluginConfiguration pluginConfiguration = null;
        private HttpClient client = null;

        public IController ControllerInstance => LegislatorController.CreatePluginController();
        public ILegislatorApiController ApiController => apiController;

        public string PluginId => AssemblyHelper.GetGuid();
        public string PluginName => AssemblyHelper.GetTitle();
        public string PluginDescription => AssemblyHelper.GetDescription();
        public Version PluginVersion => AssemblyHelper.GetVersion();
        public System.Drawing.Bitmap PluginIcon => Properties.Resources.Plugin32;
        public string PluginManufacturer => AssemblyHelper.GetManufacturer();
        public bool RegistrationRequired => false;
        public bool IsRegister => false;
        public string MoreInfoUrl => "https://itorg.pl/";
        public PluginConfiguration Configuration => pluginConfiguration;
        public bool UsePlugin => pluginConfiguration.UsePlugin;
        public bool AlwaysFullSaveDocument => false;

        public Plugin() {
            m_ThisPlugin = this;
            pluginConfiguration = PluginConfiguration.GetConfiguration();

            if (ControllerInstance.LegislatorPath.IsNotNullOrEmpty()) {
                var invoker = new Invoker(ControllerInstance.LegislatorPath);
                apiController = invoker.GetStaticPropertyValue<ILegislatorApiController>("EAP.UI.exe", "LegislatorApiController", "Singleton");
            }

            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("X-MicrosoftAjax", "Delta=true");

            client.Timeout = TimeSpan.FromSeconds(60 * 60);
        }

        public System.Windows.Forms.UserControl GetAboutBoxControl() => null;
        public System.Windows.Forms.UserControl GetPluginOptionsControl() => new PluginOptionsControl();
        public System.Windows.Forms.UserControl GetPluginRegistrationControl() => null;

        internal void SaveConfiguration() { PluginConfiguration.SaveConfiguration(pluginConfiguration); }

        public void BeforeDocumentOpen(string filePath, params object[] args) { }

        public void DocumentHasBeenSaved(string filePath) {
            // SendDocument(filePath);
        }

        public void DocumentWasLocked(string filePath) {
            // SendDocument(filePath);
        }

        public void DocumentWasSign(string filePath) {
            // SendDocument(filePath);
        }

        // -----------------------------------------------------------------------------------------------

        public async Task<ActionResult> SendDocument(string filePath) {
            if (!UsePlugin) { return new ActionResult(); }

            if (ApiController.IsNull()) { throw new Exception("ApiController is not initialized!"); }
            if (Configuration.IsNull()) { throw new Exception("Configuration is not initialized!"); }

            // zdjęcie blokady z pliku ZIPX
            ApiController.RemoveLockFile(filePath);

            // pobranie danych metrykowych pliku ZIPX
            var markInfo = ApiController.GetMarkInfo(filePath);
            if (markInfo.IsNull()) { throw new Exception("Nie udało się pobrać informacji o metadanych dokumentu!"); }

            var values = new Dictionary<string, string>
            {
                { "id", "XXXX-XXXXX" },
                { "kod", "TOKEN" },
                { "gid", ApiController.GetUpeId().ToString() },
                { "nazwa", GetDocumentDescription(markInfo.FullTitle) },
                { "user", ApiController.GetUserName() },
                { "zid", markInfo.Id },
                { "plik", Path.GetFileName(filePath) },
                { "base64", Convert.ToBase64String(File.ReadAllBytes(filePath)) }
            };


            var content = new FormUrlEncodedContentEx(values);

            var url = "http://url-do-waszego-systemu"; // Można zapisać na stałe w konfiguracji Configuration.UploadDocumentUrl;
            if (!url.EndsWith("sendDocument")) {
                if (!url.EndsWith("/")) {
                    url += "/sendDocument";
                }
                else {
                    url += "sendDocument";
                }
            }

            var response = await client.PostAsync(url, content);
            if (response.IsSuccessStatusCode) {
                var responseString = await response.Content.ReadAsStringAsync();

                if (responseString.IsNotNullOrEmpty() && !responseString.Contains("error")) {
                    var xml = System.Xml.Linq.XElement.Parse(responseString);
                    return new ActionResult() { Success = true, Message = $"Dokument oznaczony numerem ... został przesłany do repozytorium dokumentów programu... ." };
                }
                else if (responseString.IsNotNullOrEmpty() && responseString.Contains("error")) {
                    var xml = System.Xml.Linq.XElement.Parse(responseString);
                    return new ActionResult() { Success = false, Message = PluginName + "\r\n" + xml.Value };
                }
            }

            return new ActionResult();
        }

        private string GetDocumentDescription(MarkFullTitleInfo e) {
            if (e.IsNotNull()) {
                var name = String.Empty;
                name += e.TypeText;
                if (e.Numbers.IsNotNull() && e.Numbers.Count > 0) {
                    name += (" " + e.Numbers.First().Number);
                }
                if (e.LegislativeBodies.IsNotNull() && e.LegislativeBodies.Count > 0) {
                    name += (" " + e.LegislativeBodies.First().NameGenitive);
                }
                if (e.Date != DateTime.MinValue) {
                    name += e.Date.GetDateName(" z dnia");
                }
                if (e.Title.IsNotNullOrEmpty()) {
                    name += (" " + e.Title);
                }

                return name;
            }
            return "akt";
        }
    }
}