using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace RefScout.IPC.Server
{
    public class PipeIpcServer : IIpcServer
    {
        private readonly int _port;

        public PipeIpcServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            // started for each client in listen
        }

        public async IAsyncEnumerable<IIpcRequest> Listen()
        {
            while (true)
            {
                using var server = new NamedPipeServerStream($"PipeIpc-{_port}");
                await server.WaitForConnectionAsync().ConfigureAwait(false);

                var reader = new StreamReader(server);
                var writer = new StreamWriter(server);
                while (server.IsConnected)
                {
                    var message = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }

                    yield return new PipeIpcRequest(message, writer);

                    await writer.FlushAsync().ConfigureAwait(false);
                    server.WaitForPipeDrain();
                }

                try
                {
                    reader.Dispose();
                    writer.Dispose();
                }
                catch
                {
                    // pass
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }
}