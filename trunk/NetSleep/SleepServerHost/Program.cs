using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCornell.NetSleep.SleepServerHost
{
    /// <summary>
    /// Extremely simplistic console application that just runs a standalone SleepServer instance. Intended for testing/debugging purposes.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Default for testing
            int port = 12345;

            // Let the user pass in a port number
            if (args.Length > 1)
            {
                try
                {
                    port = Int32.Parse(args[0]);
                    if (port < 1 || port > 65534)
                        throw new ArgumentOutOfRangeException("port", "TCP port number must be in the range of 1 to 65534");
                }
                catch
                {
                    Console.WriteLine("Usage: SleepServerHost [port]");
                    Console.WriteLine("\nWhere port is the TCP port number to listen on (1-65534). Defaults to 12345.");
                    return;
                }
            }

            var srv = new SleepServer(port);
            srv.Start();
            Console.WriteLine("Server started on port {0}. Press Enter to exit...", port);
            Console.ReadLine();
            srv.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
