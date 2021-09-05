using DiscordMusicBot_dotNet.Core;
using System.Text;

namespace DiscordMusicBot_dotNet {
    class Program {

        static void Main(string[] args) {
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            new Main().MainAsync().GetAwaiter().GetResult();
        }   
    }
}
