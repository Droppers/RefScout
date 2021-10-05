using System.Collections.Generic;

namespace RefScout.IPC.Server
{
    public interface IIpcServer
    {
        void Start();

        IAsyncEnumerable<IIpcRequest> Listen();
    }
}