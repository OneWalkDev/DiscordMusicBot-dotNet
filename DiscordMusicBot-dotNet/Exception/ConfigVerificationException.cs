using System;
using System.Runtime.Serialization;

namespace DiscordMusicBot_dotNet.Exception {

    [Serializable()]
    public class ConfigVerificationException : System.Exception {

        public ConfigVerificationException(): base() {
        }

        public ConfigVerificationException(string message)
            : base(message) {
        }

        public ConfigVerificationException(string message, System.Exception innerException)
            : base(message, innerException) {
        }

        protected ConfigVerificationException(SerializationInfo info, StreamingContext context): base(info, context) {
        }

    }
}
