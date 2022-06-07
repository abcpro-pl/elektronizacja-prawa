/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace MyFirstPlugin {
    [XmlRoot("PluginConfiguration")]
    public class PluginConfiguration {
        [XmlElement("UsePlugin")] public bool UsePlugin { get; set; }

        internal static PluginConfiguration GetConfiguration() {
            var configPath = GetConfigurationFilePath();
            if (File.Exists(configPath)) {
                XmlSerializer serializer = new XmlSerializer(typeof(PluginConfiguration));
                return serializer.Deserialize(new MemoryStream(File.ReadAllBytes(configPath))) as PluginConfiguration;
            }

            return new PluginConfiguration() { UsePlugin = false };
        }

        internal static bool SaveConfiguration(PluginConfiguration conf) {
            var configPath = GetConfigurationFilePath();

            XmlSerializer serializer = new XmlSerializer(typeof(PluginConfiguration));
            var ms = new MemoryStream();
            serializer.Serialize(ms, conf);

            File.WriteAllBytes(configPath, ms.ToArray());

            return true;
        }

        public static string GetConfigurationFilePath() {
            return Path.Combine(Plugin.ThisPlugin.ControllerInstance.GetDefaultConfigurationDirectory(), Assembly.GetExecutingAssembly().GetName().Name + ".xml");
        }
    }
}
