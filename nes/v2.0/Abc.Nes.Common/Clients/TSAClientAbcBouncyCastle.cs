using Abc.Nes.Common.Helpers;
using iText.Signatures;
using Org.BouncyCastle.Tsp;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.Common.Clients {
    public class TSAClientAbcBouncyCastle : TSAClientBouncyCastle {
        X509Certificate2 certificate = null;
        string hashAlgorithm = HashAlgorithmName.SHA256.Name;
        string digestOid = string.Empty;
        byte[] nonce = null;
        bool certReq;

        public TSAClientAbcBouncyCastle(string url) : base(url) { }
        public TSAClientAbcBouncyCastle(string url, X509Certificate2 cert)
            : base(url) {
            certificate = cert;
        }
        public TSAClientAbcBouncyCastle(string url, X509Certificate2 cert, string digestAlgorithm)
            : this(url, cert) {
            SetHashAlgorithm(digestAlgorithm);
        }
        public TSAClientAbcBouncyCastle(string url, string username, string password) : base(url, username, password) { }
        public TSAClientAbcBouncyCastle(string url, string username, string password, int tokSzEstimate, string digestAlgorithm) :base(url, username, password, tokSzEstimate, digestAlgorithm) {
            SetHashAlgorithm(digestAlgorithm);
        }


        public void SetHashAlgorithm(string alg) {
            hashAlgorithm = alg;
            digestOid = CryptoConfig.MapNameToOID(alg);
        }
        public void SetCertificate(X509Certificate2 cert) {
            certificate = cert;
        }
        public void SetNonce(byte[] nonceBytes) {
            nonce = nonceBytes;
        }
        public void SetCertReq(bool flag) {
            certReq = flag;
        }

        //public override bool Equals(object obj) {
        //    return base.Equals(obj);
        //}

        //public override int GetHashCode() {
        //    return base.GetHashCode();
        //}

        //public override IDigest GetMessageDigest() {
        //    return base.GetMessageDigest();
        //}

        //public override byte[] GetTimeStampToken(byte[] imprint) {
        //    return base.GetTimeStampToken(imprint);
        //}

        //public override int GetTokenSizeEstimate() {
        //    return base.GetTokenSizeEstimate();
        //}

        //public override string GetTSAReqPolicy() {
        //    return base.GetTSAReqPolicy();
        //}

        //public override void SetTSAInfo(ITSAInfoBouncyCastle tsaInfo) {
        //    base.SetTSAInfo(tsaInfo);
        //}

        //public override void SetTSAReqPolicy(string tsaReqPolicy) {
        //    base.SetTSAReqPolicy(tsaReqPolicy);
        //}

        //public override string ToString() {
        //    return base.ToString();
        //}

        protected override byte[] GetTSAResponse(byte[] requestBytes) {

            var tsreq = new TimeStampRequest(requestBytes);

            var tsaResponse = TimestampRequestHelper.GetTSAResponse(
                tsreq, tsaURL, tsaUsername, tsaPassword, certificate
            );

            var tsResp = new TimeStampResponse(tsaResponse);
            var encoded = tsResp.GetEncoded();

            return encoded;

        }
        internal class TsaResponse {
            internal string encoding;
            internal Stream tsaResponseStream;
        }
    }

    public static class SignExtensions {
        public static int JRead(this Stream stream, byte[] buffer, int offset, int count) {
            int result = stream.Read(buffer, offset, count);
            return result == 0 ? -1 : result;
        }
    }
}
