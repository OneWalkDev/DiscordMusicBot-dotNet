using System;
using System.Runtime.Serialization;

namespace DiscordMusicBot_dotNet.Exception {

    [Serializable()]
    public class SearchNotFoundException : System.Exception {

        public SearchNotFoundException()
            : base() {
        }

        public SearchNotFoundException(string message)
            : base(message) {
        }

        public SearchNotFoundException(string message, System.Exception innerException)
            : base(message, innerException) {
        }

        protected SearchNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}