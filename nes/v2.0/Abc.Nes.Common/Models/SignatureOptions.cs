using System;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.Common.Models {
    public class SignatureOptions {
        public X509Certificate2 Certificate { get; set; } = null;
        public CommitmentTypeId Reason { get; set; } = CommitmentTypeId.ProofOfOrigin;
        public string Location { get; set; } = null;
        public DateTime? SignDate { get; set; } = DateTime.Now;
        public bool AddTimestamp {
            get {
                return TimestampOptions != null && TimestampOptions.TsaUrl != null && TimestampOptions.TsaUrl != string.Empty;
            }
        }
        public TimestampOptions TimestampOptions { get; set; } = null;
        /// <summary>
        /// Custom name, if empty name is taken from certificate
        /// </summary>
        public string SignerName { get; set; } = null;
        public string HashAlgorithmName { get; set; } = null;
    }
}
