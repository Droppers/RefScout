using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RefScout.IPC.Client
{
    public class TcpIpcClient : IpcClient
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        public override string Send(string message)
        {
            if (_client == null || _stream == null)
            {
                _client = new TcpClient();
                _client.Connect("127.0.0.1", Port);
                _stream = _client.GetStream();
            }

            var buffer = Encoding.ASCII.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);

            var responseBuffer = new byte[4096];
            var read = _stream.Read(responseBuffer, 0, responseBuffer.Length);

            return Encoding.ASCII.GetString(responseBuffer, 0, read);
        }

        public override async Task<string> SendAsync(string message)
        {
            if (_client == null || _stream == null)
            {
                _client = new TcpClient();
                await _client.ConnectAsync("127.0.0.1", Port).ConfigureAwait(false);
                _stream = _client.GetStream();
            }

            var buffer = Encoding.ASCII.GetBytes(message);
            await _stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

            var responseBuffer = new byte[4096];
            var read = await _stream.ReadAsync(responseBuffer, 0, responseBuffer.Length).ConfigureAwait(false);

            return Encoding.ASCII.GetString(responseBuffer, 0, read);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            base.Dispose(disposing);
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}