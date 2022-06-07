/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MyFirstPlugin.Utils {
    static class AssemblyHelper {
        public static string GetGuid() {
            var assembly = Assembly.GetExecutingAssembly();

            var objects = assembly.GetCustomAttributes(typeof(GuidAttribute),
                false);
            if (objects.Length > 0) {
                return ((GuidAttribute)objects[0]).Value;
            }
            else {
                return String.Empty;
            }
        }
        public static string GetDescription() {
            var assembly = Assembly.GetExecutingAssembly();

            var objects = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute),
                false);
            if (objects.Length > 0) {
                return ((AssemblyDescriptionAttribute)objects[0]).Description;
            }
            else {
                return String.Empty;
            }
        }
        public static string GetTitle() {
            var assembly = Assembly.GetExecutingAssembly();

            var objects = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute),
                false);
            if (objects.Length > 0) {
                return ((AssemblyTitleAttribute)objects[0]).Title;
            }
            else {
                return String.Empty;
            }
        }
        public static Version GetVersion() {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
        public static string GetManufacturer() {
            var assembly = Assembly.GetExecutingAssembly();

            var objects = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute),
                false);
            if (objects.Length > 0) {
                return ((AssemblyCompanyAttribute)objects[0]).Company;
            }
            else {
                return String.Empty;
            }
        }
    }
}
