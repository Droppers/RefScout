using System.IO;
using System.Threading.Tasks;

namespace RefScout.IPC.Server
{
    public record PipeIpcRequest(string Message, StreamWriter Writer) : IIpcRequest
    {
        public async Task Reply(string message)
        {
            await Writer.WriteLineAsync(message).ConfigureAwait(false);
        }
    }
}