using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RefScout.IPC.Client
{
    public abstract class IpcClient : IIpcClient
    {
        private Process? _process;
        public int Port { get; set; }

        public bool Started { get; set; }

        public void Start(string executableFileName, int? port = null)
        {
            Port = port ?? FreeTcpPort();
            _process = new Process
            {
                StartInfo = new ProcessStartInfo(executableFileName, Port.ToString())
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                }
            };
            _process.Exited += (_, _) => _process = null;
            _process.Start();

            Started = true;
        }

        public abstract string Send(string message);

        public abstract Task<string> SendAsync(string message);

        private static int FreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_process == null)
            {
                return;
            }

            try
            {
                Send("exit");
                _process.WaitForExit();
            }
            finally
            {
                _process.Dispose();
            }
        }
    }
}