using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RefScout.IPC.Server
{
    public record TcpIpcRequest(string Message, NetworkStream Stream) : IIpcRequest
    {
        public async Task Reply(string message)
        {
            var response = Encoding.UTF8.GetBytes(message);
            await Stream.WriteAsync(response, 0, response.Length);
        }
    }
}