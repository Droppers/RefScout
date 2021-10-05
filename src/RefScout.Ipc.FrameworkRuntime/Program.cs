using System;
using System.Threading.Tasks;
using RefScout.IPC.Server;

namespace RefScout.IPC.FrameworkRuntime
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var port = int.Parse(args[0]);

            var server = new PipeIpcServer(port);
            server.Start();

            Console.WriteLine($"Listening on port {port}");

            await foreach (var request in server.Listen())
            {
                if (request.Message == "exit")
                {
                    await request.Reply("goodbye");
                    break;
                }

                try
                {
                    var context = AppDomain.CreateDomain("fake domain");
                    var assembly = context.Load(request.Message);
                    AppDomain.Unload(context);
                    await request.Reply($"{assembly.GetName().Version}|{assembly.Location}");
                }
                catch
                {
                    await request.Reply("error");
                }
            }
        }
    }
}