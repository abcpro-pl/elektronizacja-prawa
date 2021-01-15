using System;
using System.Collections.Generic;
using System.Linq;

namespace Abc.Nes.Xades.Utils {
    static class TypeExtensions {
        public static Type FindTypeInAllAssemblies(this string typeName) {
            var type = Type.GetType(typeName);
            if (type != null) { return type; }

            var list = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblies != null) {
                foreach (var assembly in assemblies) {
                    try {
                        var types = assembly.GetExportedTypes();
                        var t = types.Where(x => x.FullName.Contains(typeName)).FirstOrDefault();
                        if (t != null) {
                            list.Add(t); ;
                        }
                    }
                    catch { }
                }
            }

            if (list.Count > 0) {
                return list.First();
            }

            return default;
        }
    }
}
