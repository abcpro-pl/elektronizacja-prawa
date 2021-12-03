// --------------------------------------------------------------------------------------------------------------------
// TimeStampClient.cs
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

using Abc.Nes.Common.Helpers;
using Abc.Nes.Xades.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Abc.Nes.Xades.Clients {
    internal class TimeStampClient : ITimeStampClient {
        #region Private variables
        private string _url;
        private string _user;
        private string _password;
        private string _reqPolicy;
        private X509Certificate2 _certificate;
        #endregion

        #region Constructors

        public TimeStampClient(string url) {
            _url = url;
        }

        public TimeStampClient(string url, string reqPolicy) 
            : this(url) {
            _reqPolicy = reqPolicy;
        }
        /// <summary>
        /// Time stamp client with credential authorized request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="reqPolicy"></param>
        public TimeStampClient(string url, string user, string password, string reqPolicy = null)
            : this(url, reqPolicy) {
            _user = user;
            _password = password;
        }

        /// <summary>
        /// Time stamp client with request signed by certificate
        /// </summary>
        /// <param name="url"></param>
        /// <param name="certificate"></param>
        /// <param name="reqPolicy"></param>
        public TimeStampClient(string url, X509Certificate2 certificate, string reqPolicy = null)
            : this(url, reqPolicy) {
            _certificate = certificate;
        }


        #endregion

        #region Public methods

        /// <summary>
        /// Realiza la petición de sellado del hash que se pasa como parametro y devuelve la
        /// respuesta del servidor.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="digestMethod"></param>
        /// <param name="certReq"></param>
        /// <returns></returns>
        public byte[] GetTimeStamp(byte[] hash, DigestMethod digestMethod, bool certReq) {
            var nonce = BigInteger.ValueOf(DateTime.Now.Ticks);
            
            var tsBytes = TimestampRequestHelper.RequestTimestamp(hash, _url, digestMethod.Oid, certReq, _reqPolicy, _user,_password, _certificate, nonce);
            
            return tsBytes;
            
        }

        #endregion
    }
}
