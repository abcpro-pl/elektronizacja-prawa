/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MyFirstPlugin.Utils {
    internal class FormUrlEncodedContentEx : ByteArrayContent {

        // Note that using a Dictionary<string, string> doesn't allow duplicate key values. However, 
        // users can use List<KeyValuePair<string, string>> to provide lists with multiple names.

        public FormUrlEncodedContentEx(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
            : base(GetContentByteArray(nameValueCollection)) {
            Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }

        private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection) {
            if (nameValueCollection == null) {
                throw new ArgumentNullException("nameValueCollection");
            }
            Contract.EndContractBlock();

            // Encode and concatenate data
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in nameValueCollection) {
                if (builder.Length > 0) {
                    // Not first, add a seperator
                    builder.Append('&');
                }

                if (pair.Key == "base64") {
                    builder.Append(Encode(pair.Key));
                    builder.Append('=');
                    builder.Append(pair.Value);
                }
                else {
                    builder.Append(Encode(pair.Key));
                    builder.Append('=');
                    builder.Append(Encode(pair.Value));
                }
            }

            return HttpRuleParser.DefaultHttpEncoding.GetBytes(builder.ToString());
        }

        private static string Encode(string data) {
            if (String.IsNullOrEmpty(data)) {
                return String.Empty;
            }
            // Escape spaces as '+'.
            return Uri.EscapeDataString(data).Replace("%20", "+");
        }
    }
}
