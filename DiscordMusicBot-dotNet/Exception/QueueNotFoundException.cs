using System;
using System.Runtime.Serialization;

namespace DiscordMusicBot_dotNet.Exception {

    [Serializable()]
    public class QueueNotFoundException : System.Exception {

        public QueueNotFoundException()
            : base() {
        }

        public QueueNotFoundException(string message)
            : base(message) {
        }

        public QueueNotFoundException(string message, System.Exception innerException)
            : base(message, innerException) {
        }

        protected QueueNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}