/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


namespace Abc.Nes.Validators {
    class ValidationResultItem : IValidationResultItem {
        public ValidationResultSource Source { get; set; }
        public ValidationResultType Type { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string DefaultMessage { get; set; }
    }
}
