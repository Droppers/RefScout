using System;
using System.Threading.Tasks;

namespace RefScout.IPC.Client
{
    public interface IIpcClient : IDisposable
    {
        bool Started { get; }

        void Start(string executableFileName, int? port = null);

        string Send(string message);

        Task<string> SendAsync(string message);
    }
}