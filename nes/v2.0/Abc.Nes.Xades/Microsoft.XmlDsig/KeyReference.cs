// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.XmlDsig {
    internal sealed class KeyReference : EncryptedReference {
        public KeyReference() : base() {
            ReferenceType = "KeyReference";
        }

        public KeyReference(string uri) : base(uri) {
            ReferenceType = "KeyReference";
        }

        public KeyReference(string uri, TransformChain transformChain) : base(uri, transformChain) {
            ReferenceType = "KeyReference";
        }
    }
}
