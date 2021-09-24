using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.Cryptography {
    public class CertificateValidationInfo {
        public bool IsValid { get; }
        public X509ChainStatusFlags Status { get; }
        public string StatusTitle { get; }
        public string Message { get; }
        public string StatusInformation { get; }
        public string ErrorMessage { get; }
        private CertificateValidationInfo() {
            Status = X509ChainStatusFlags.NoError;
            Message = string.Empty;
            StatusInformation = string.Empty;
            StatusTitle = string.Empty;
            ErrorMessage = string.Empty;
        }
        public CertificateValidationInfo(bool isValid) : this() {
            IsValid = isValid;
        }
        public CertificateValidationInfo(X509ChainStatus status, string userName) : this() {
            if (status.Status == X509ChainStatusFlags.NoError) {
                IsValid = true;
            }
            else {
                IsValid = false;
                Status = status.Status;
                StatusTitle = GetX509ChainStatusFlagsTitle(status.Status);
                StatusInformation = status.StatusInformation;
                Message = $"[{StatusTitle}] {GetX509ChainStatusFlagsMessage(status.Status)}";
                ErrorMessage = $"Weryfikacja certyfikatu osoby podpisującej {userName} zakończona niepowodzeniem: {Message}".Trim();
            }
        }
        private string GetX509ChainStatusFlagsMessage(X509ChainStatusFlags flag) {
            switch (flag) {
                case X509ChainStatusFlags.HasWeakSignature: { return "Certyfikat nie został podpisany z użyciem właściwego algorytmu. Prawdopodobnie do utworzenia skrótu certyfikatu użyto algorytmów mieszania MD2 lub MD5."; }
                case X509ChainStatusFlags.NotSignatureValid: { return "Łańcuch X509 jest nieprawidłowy z powodu nieprawidłowego podpisu certyfikatu."; }
                case X509ChainStatusFlags.CtlNotSignatureValid: { return "Lista zaufanych certyfikatów (CTL) zawiera nieprawidłowy podpis."; }

                case X509ChainStatusFlags.NotTimeNested: { return "Certyfikat CA (urzędu certyfikacji) i wystawiony certyfikat mają okresy ważności, które nie są w sobie zawarte."; }
                case X509ChainStatusFlags.NotTimeValid: { return "Łańcuch X509 jest nieprawidłowy z powodu nieprawidłowej wartości czasu, takiej jak wartość wskazująca, że certyfikat wygasł."; }
                case X509ChainStatusFlags.CtlNotTimeValid: { return "Lista zaufania certyfikatów (CTL) jest nieprawidłowa z powodu nieprawidłowej wartości czasu, takiej jak ta, która wskazuje, że lista CTL wygasła."; }

                case X509ChainStatusFlags.NotValidForUsage: { return "Użycie klucza jest nieprawidłowe."; }
                case X509ChainStatusFlags.CtlNotValidForUsage: { return "Lista zaufania certyfikatów (CTL) jest nieprawidłowa do tego użytku."; }

                case X509ChainStatusFlags.HasNotSupportedCriticalExtension: { return "Certyfikat nie obsługuje krytycznego rozszerzenia."; }
                case X509ChainStatusFlags.InvalidExtension: { return "Łańcuch X509 jest nieprawidłowy z powodu nieprawidłowego rozszerzenia."; }

                case X509ChainStatusFlags.Cyclic: { return "Nie można zbudować łańcucha X509."; }

                case X509ChainStatusFlags.Revoked: { return "Łańcuch X509 jest nieprawidłowy z powodu odwołanego certyfikatu."; }
                case X509ChainStatusFlags.RevocationStatusUnknown: { return "Nie można określić, czy certyfikat został odwołany. Może to być spowodowane tym, że lista odwołania certyfikatów (CRL) jest w trybie offline lub jest niedostępna."; }
                case X509ChainStatusFlags.OfflineRevocation: { return "Lista odwołania certyfikatów online (CRL), na której opiera się łańcuch X509, jest obecnie w trybie offline."; }

                case X509ChainStatusFlags.HasExcludedNameConstraint: { return "Łańcuch X509 jest nieprawidłowy, ponieważ certyfikat wykluczył ograniczenie nazwy."; }
                case X509ChainStatusFlags.PartialChain: { return "Nie można zbudować łańcucha X509 do certyfikatu głównego. "; }

                case X509ChainStatusFlags.ExplicitDistrust: { return "Certyfikat jest jawnie niezaufany."; }
                case X509ChainStatusFlags.UntrustedRoot: { return "Łańcuch X509 jest nieprawidłowy z powodu niezaufanego certyfikatu głównego. "; }

                case X509ChainStatusFlags.InvalidNameConstraints: { return "Łańcuch X509 jest nieprawidłowy z powodu nieprawidłowych ograniczeń dotyczących nazwy."; }
                case X509ChainStatusFlags.InvalidBasicConstraints: { return "Łańcuch X509 jest nieprawidłowy z powodu nieprawidłowych ograniczeń podstawowych."; }
                case X509ChainStatusFlags.HasNotDefinedNameConstraint: { return "Certyfikat ma niezdefiniowane ograniczenie dotyczące nazwy."; }
                case X509ChainStatusFlags.HasNotPermittedNameConstraint: { return "Certyfikat ma niedopuszczalne ograniczenie dotyczące nazwy."; }
                case X509ChainStatusFlags.HasNotSupportedNameConstraint: { return "Certyfikat nie ma obsługiwanego ograniczenia dotyczącego nazwy lub zawiera nieobsługiwane ograniczenie dotyczące nazwy."; }

                case X509ChainStatusFlags.NoIssuanceChainPolicy: { return "W certyfikacie nie ma rozszerzenia zasad certyfikatów. Ten błąd wystąpiłby, gdyby zasady grupy określały, że wszystkie certyfikaty muszą mieć zasady certyfikatów. "; }
                case X509ChainStatusFlags.InvalidPolicyConstraints: { return "Łańcuch X509 jest nieprawidłowy z powodu nieprawidłowych ograniczeń zasad."; }

                default: { return flag.ToString(); }
            }
        }
        private string GetX509ChainStatusFlagsTitle(X509ChainStatusFlags flag) {
            switch (flag) {
                case X509ChainStatusFlags.HasWeakSignature:
                case X509ChainStatusFlags.NotSignatureValid:
                case X509ChainStatusFlags.CtlNotSignatureValid: { return "Sygnatura"; }

                case X509ChainStatusFlags.NotTimeNested:
                case X509ChainStatusFlags.NotTimeValid:
                case X509ChainStatusFlags.CtlNotTimeValid: { return "Ważność certyfikatu"; }

                case X509ChainStatusFlags.NotValidForUsage:
                case X509ChainStatusFlags.CtlNotValidForUsage: { return "Zastosowanie"; }

                case X509ChainStatusFlags.HasNotSupportedCriticalExtension:
                case X509ChainStatusFlags.InvalidExtension: { return "Rozszerzenie"; }

                case X509ChainStatusFlags.Cyclic: { return "Zapętlenie"; }

                case X509ChainStatusFlags.Revoked:
                case X509ChainStatusFlags.RevocationStatusUnknown:
                case X509ChainStatusFlags.OfflineRevocation: { return "Odwołanie certyfikatu"; }

                case X509ChainStatusFlags.HasExcludedNameConstraint:
                case X509ChainStatusFlags.PartialChain: { return "Łańcuch certyfikatów"; }

                case X509ChainStatusFlags.ExplicitDistrust:
                case X509ChainStatusFlags.UntrustedRoot: { return "Źródło"; }

                case X509ChainStatusFlags.InvalidNameConstraints:
                case X509ChainStatusFlags.InvalidBasicConstraints:
                case X509ChainStatusFlags.HasNotDefinedNameConstraint:
                case X509ChainStatusFlags.HasNotPermittedNameConstraint:
                case X509ChainStatusFlags.HasNotSupportedNameConstraint: { return "Wymagalność danych"; }

                case X509ChainStatusFlags.NoIssuanceChainPolicy:
                case X509ChainStatusFlags.InvalidPolicyConstraints: { return "Polityka certyfikatu"; }

                default: { return flag.ToString(); }
            }
        }
    }
}
