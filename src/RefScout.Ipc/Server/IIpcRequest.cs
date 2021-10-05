using System.Threading.Tasks;

namespace RefScout.IPC.Server
{
    public interface IIpcRequest
    {
        string Message { get; }
        Task Reply(string message);
    }
}