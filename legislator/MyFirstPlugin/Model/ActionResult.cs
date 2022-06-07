/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;

namespace MyFirstPlugin.Model {
    public sealed class ActionResult {
        public bool Success { get; set; }
        public string Message { get; set; } = String.Empty;

    }
}
