using System;

namespace Abc.Nes.Exceptions {
    public class DocumentSchemaException : Exception {
        public DocumentSchemaException() {
        }

        public DocumentSchemaException(string message) : base(message) {
        }

        public DocumentSchemaException(string message, Exception inner) : base(message, inner) {
        }
    }
}
