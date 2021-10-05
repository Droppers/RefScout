using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace RefScout.IPC.Client
{
    public class PipeIpcClient : IpcClient
    {
        private NamedPipeClientStream? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        public override string Send(string message)
        {
            if (!Started)
            {
                throw new Exception("IPC client has not been started, call Start() first.");
            }

            if (_client == null || _reader == null || _writer == null)
            {
                _client = new NamedPipeClientStream($"PipeIpc-{Port}");
                _client.Connect(3000);
                _reader = new StreamReader(_client);
                _writer = new StreamWriter(_client);
            }

            _writer.WriteLine(message);
            _writer.Flush();
            return _reader.ReadLine()!;
        }

        public override async Task<string> SendAsync(string message)
        {
            if (!Started)
            {
                throw new Exception("IPC client has not been started, call Start() first.");
            }

            if (_client == null)
            {
                _client = new NamedPipeClientStream($"PipeIpc-{Port}");
                await _client.ConnectAsync().ConfigureAwait(false);
            }

            using var reader = new StreamReader(_client);
            using var writer = new StreamWriter(_client);

            await writer.WriteLineAsync(message).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            return await reader.ReadLineAsync().ConfigureAwait(false);
        }
    }
}