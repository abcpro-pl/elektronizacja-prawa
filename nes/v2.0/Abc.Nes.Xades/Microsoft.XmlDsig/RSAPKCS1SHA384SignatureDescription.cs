// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Cryptography;

namespace Microsoft.XmlDsig {
    internal class RSAPKCS1SHA384SignatureDescription : RSAPKCS1SignatureDescription
    {
        public RSAPKCS1SHA384SignatureDescription() : base("SHA384")
        {
        }

        public sealed override HashAlgorithm CreateDigest()
        {
            return SHA384.Create();
        }
    }
}
