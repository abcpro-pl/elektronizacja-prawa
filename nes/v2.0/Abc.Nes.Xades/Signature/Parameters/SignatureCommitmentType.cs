// --------------------------------------------------------------------------------------------------------------------
// SignatureCommitmentType.cs
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
// along with this program.  If not, see https://www.gnu.org/licenses/lgpl-3.0.txt. 
//
// E-Mail: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace Abc.Nes.Xades.Signature.Parameters {
    internal class SignatureCommitmentType {
        public static SignatureCommitmentType ProofOfOrigin = new SignatureCommitmentType(CommitmentTypeId.ProofOfOrigin);
        public static SignatureCommitmentType ProofOfReceipt = new SignatureCommitmentType(CommitmentTypeId.ProofOfReceipt);
        public static SignatureCommitmentType ProofOfDelivery = new SignatureCommitmentType(CommitmentTypeId.ProofOfDelivery);
        public static SignatureCommitmentType ProofOfSender = new SignatureCommitmentType(CommitmentTypeId.ProofOfSender);
        public static SignatureCommitmentType ProofOfApproval = new SignatureCommitmentType(CommitmentTypeId.ProofOfApproval);
        public static SignatureCommitmentType ProofOfCreation = new SignatureCommitmentType(CommitmentTypeId.ProofOfCreation);

        public string URI { get; }
        public string Description { get; }


        public SignatureCommitmentType(CommitmentTypeId id) {
            URI = id.GetCategory();
            Description = id.GetDescription();
        }
    }

    public static class __EnumExtensions {
        public static string GetCategory(this Enum value) {
            try {
                var fi = value.GetType().GetField(value.ToString());
                CategoryAttribute[] attributes = (CategoryAttribute[])fi.GetCustomAttributes(typeof(CategoryAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Category : value.ToString();
            }
            catch {
                return string.Empty;
            }
        }
        public static string GetDescription(this Enum value) {
            try {
                var fi = value.GetType().GetField(value.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
            }
            catch {
                return string.Empty;
            }
        }
    }

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
