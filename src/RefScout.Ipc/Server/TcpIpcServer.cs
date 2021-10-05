using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RefScout.IPC.Server
{
    public class TcpIpcServer : IIpcServer
    {
        private readonly int _port;
        private TcpListener? _listener;

        public TcpIpcServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
        }

        public async IAsyncEnumerable<IIpcRequest> Listen()
        {
            if (_listener == null)
            {
                throw new InvalidOperationException("Call Start() before listening for requests.");
            }

            while (true)
            {
                using var tcpClient = await _listener.AcceptTcpClientAsync();
                using var networkStream = tcpClient.GetStream();

                while (true)
                {
                    string? message;
                    try
                    {
                        var buffer = new byte[4096];
                        var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        message = Encoding.ASCII.GetString(buffer, 0, byteCount);
                    }
                    catch
                    {
                        break;
                    }

                    yield return new TcpIpcRequest(message, networkStream);
                }
            }
        }
    }
}