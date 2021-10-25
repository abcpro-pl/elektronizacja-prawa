using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Abc.Nes.Common {
    public enum CommitmentTypeId {
        [Category("http://uri.etsi.org/01903/v1.2.2#ProofOfOrigin")]
        [Description("Dowód pochodzenia (Proof of origin)")]
        ProofOfOrigin,

        [Category("http://uri.etsi.org/01903/v1.2.2#ProofOfReceipt")]
        [Description("Potwierdzenie odbioru (Proof of receipt)")]
        ProofOfReceipt,

        [Category("http://uri.etsi.org/01903/v1.2.2#ProofOfDelivery")]
        [Description("Dowód dostawy (Proof of delivery)")]
        ProofOfDelivery,

        [Category("http://uri.etsi.org/01903/v1.2.2#ProofOfSender")]
        [Description("Dowód nadawcy (Proof of sender)")]
        ProofOfSender,

        [Category("http://uri.etsi.org/01903/v1.2.2#ProofOfApproval")]
        [Description("Formalne zatwierdzenie (Proof of approval)")]
        ProofOfApproval,

        [Category("http://uri.etsi.org/01903/v1.2.2#ProofOfCreation")]
        [Description("Potwierdzenie utworzenia (Proof of creation)")]
        ProofOfCreation
    }
}
