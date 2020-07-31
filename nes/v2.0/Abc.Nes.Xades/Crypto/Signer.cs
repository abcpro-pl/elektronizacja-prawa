// --------------------------------------------------------------------------------------------------------------------
// Signer.cs
//
// FirmaXadesNet - Librería para la generación de firmas XADES
// Copyright (C) 2016 Dpto. de Nuevas Tecnologías de la Dirección General de Urbanismo del Ayto. de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

// Modified by ITORG Krzysztof Radzimski

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.Xades.Crypto {
    internal class Signer : IDisposable {
        private bool _disposeCryptoProvider;

        public X509Certificate2 Certificate { get; private set; }
        public AsymmetricAlgorithm SigningKey { get; private set; }

        public Signer(X509Certificate2 certificate) {
            if (certificate == null) {
                throw new ArgumentNullException("certificate");
            }

            if (!certificate.HasPrivateKey) {
                throw new Exception("The certificate does not contain any private keys.");
            }

            Certificate = certificate;
            SetSigningKey(Certificate);
        }

        public void Dispose() {
            if (_disposeCryptoProvider && SigningKey != null) {
                SigningKey.Dispose();
            }
        }

        private void SetSigningKey(X509Certificate2 certificate) {
            var key = certificate.GetRSAPrivateKey();
            SigningKey = key;
            _disposeCryptoProvider = false;
        }
    }
}
