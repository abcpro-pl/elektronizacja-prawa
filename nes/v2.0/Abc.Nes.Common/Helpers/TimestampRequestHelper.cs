using Abc.Nes.Common.Clients;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Abc.Nes.Common.Helpers {
    public class TimestampRequestHelper {
        public static byte[] RequestTimestamp(byte[] hash, string tspUrl, string digestMethodOid, bool certReq, string reqPolicy,
            string tspLogin, string tspPassword, X509Certificate2 certificate, BigInteger nonce) {
            TimeStampResponse tsResp = GetTimestampResponse(hash, tspUrl, digestMethodOid, certReq, reqPolicy, tspLogin, tspPassword, certificate, nonce);
            return tsResp.TimeStampToken.GetEncoded();
        }

        public static byte[] ComputeHashValue(byte[] value, string digestMethod) {
            using (var alg = GetHashAlgorithm(digestMethod)) {
                return alg.ComputeHash(value);
            }
        }

        public static byte[] GetTSAResponse(byte[] hash, string tspUrl, string digestMethodOid, bool certReq,
            string reqPolicy, string tspLogin, string tspPassword, X509Certificate2 certificate, BigInteger nonce
            ) {
            TimeStampRequest tsr = GenerateTimestampRequest(hash, digestMethodOid, certReq, reqPolicy, nonce);

            HttpWebResponse res = SendTimeStampRequest(tspUrl, tspLogin, tspPassword, certificate, tsr);

            if (res.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Serwer zwrócił nieprawidłową odpowiedź!");
            }
            else {
                byte[] respBytes = GetResponseBytes(res);
                 
                var tsRes = new TimeStampResponse(respBytes);
                tsRes.Validate(tsr);

                return respBytes;
            }
        }

        public static byte[] GetTSAResponse(TimeStampRequest tsReq, string tspUrl, string tspLogin, string tspPassword, X509Certificate2 certificate) {
            HttpWebResponse res = SendTimeStampRequest(tspUrl, tspLogin, tspPassword, certificate, tsReq);
            if (res.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Serwer zwrócił nieprawidłową odpowiedź!");
            }
            else {
                byte[] respBytes = GetResponseBytes(res);
                return respBytes;
            }
        }
        

        public static TimeStampResponse GetTimestampResponse(byte[] hash, string tspUrl, string digestMethodOid, bool certReq,
            string reqPolicy, string tspLogin, string tspPassword, X509Certificate2 certificate, BigInteger nonce) {

            TimeStampRequest tsr = GenerateTimestampRequest(hash, digestMethodOid, certReq, reqPolicy, nonce);

            var tsaResponse = GetTSAResponse(tsr, tspUrl, tspLogin, tspPassword, certificate);
            TimeStampResponse tsRes = new TimeStampResponse(tsaResponse);
            
            tsRes.Validate(tsr);

            if (tsRes.TimeStampToken == null) {
                throw new Exception("Serwer nie zwrócił sygnatury czasowej!");
            }

            return tsRes;

        }

        public static TimeStampResponse GetTimestampResponse2(byte[] hash, string tspUrl, string digestMethodOid, bool certReq, 
            string reqPolicy, string tspLogin, string tspPassword, X509Certificate2 certificate, BigInteger nonce
            ) {
            TimeStampRequest tsr = GenerateTimestampRequest(hash, digestMethodOid, certReq, reqPolicy, nonce);

            HttpWebResponse res = SendTimeStampRequest(tspUrl, tspLogin, tspPassword, certificate, tsr);

            if (res.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Serwer zwrócił nieprawidłową odpowiedź!");
            }
            else {
                Stream tsResponseStream = res.GetResponseStream();
                //Stream resStream = new BufferedStream(tsResponseStream);
                TimeStampResponse tsRes = new TimeStampResponse(tsResponseStream);
                var encoding = res.Headers[HttpResponseHeader.ContentEncoding];
                //resStream.Close();

                tsRes.Validate(tsr);

                if (tsRes.TimeStampToken == null) {
                    throw new Exception("Serwer nie zwrócił sygnatury czasowej!");
                }

                return tsRes;
            }
        }

        private static HttpWebResponse SendTimeStampRequest(string tspUrl, string tspLogin, string tspPassword, X509Certificate2 certificate, TimeStampRequest tsr) {
            byte[] bytesToSend = tsr.GetEncoded();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(tspUrl);
            req.Method = "POST";
            

            if (!string.IsNullOrEmpty(tspLogin) && !string.IsNullOrEmpty(tspPassword)) {
                //basic auth
                string auth = string.Format("{0}:{1}", tspLogin, tspPassword);
                req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(auth), Base64FormattingOptions.None);

            }
            else if (certificate != null) {
                //signed request
                ContentInfo ci = new ContentInfo(bytesToSend);
                SignedCms env = new SignedCms(ci);
                CmsSigner signer = new CmsSigner(certificate);
                env.ComputeSignature(signer, false);
                bytesToSend = env.Encode();
                //req.ContentLength = bytesToSend.Length;
            }

            req.ContentType = "application/timestamp-query";
            req.ContentLength = bytesToSend.Length;

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(bytesToSend, 0, bytesToSend.Length);
            reqStream.Close();

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res;
        }

        private static TimeStampRequest GenerateTimestampRequest(byte[] hash, string digestMethodOid, bool certReq, string reqPolicy, BigInteger nonce) {
            TimeStampRequestGenerator tsrq = new TimeStampRequestGenerator();

            if (reqPolicy != null && reqPolicy != string.Empty)
                tsrq.SetReqPolicy(reqPolicy);

            tsrq.SetCertReq(certReq);

            TimeStampRequest tsr = tsrq.Generate(digestMethodOid, hash, nonce);
            return tsr;
        }

        

        private static HashAlgorithm GetHashAlgorithm(string name) {
            if (name == "SHA1" || name == "SHA-1") {
                return SHA1.Create();
            }
            else if (name == "SHA256" || name == "SHA-256") {
                return SHA256.Create();
            }
            else if (name == "SHA512" || name == "SHA-512") {
                return SHA512.Create();
            }
            else {
                throw new Exception("Algorithm is not supported.");
            }
        }
        private static byte[] GetResponseBytes(HttpWebResponse res) {
            Stream tsResponseStream = res.GetResponseStream();
            //Stream resStream = new BufferedStream(tsResponseStream);
            MemoryStream baos = new MemoryStream();
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = tsResponseStream.JRead(buffer, 0, buffer.Length)) >= 0) {
                baos.Write(buffer, 0, bytesRead);
            }
            byte[] respBytes = baos.ToArray();
            return respBytes;
        }
    }
}
