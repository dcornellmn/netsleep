using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DCornell.NetSleep
{
    public class SleepServer
    {
        private TcpListener listener;
        private int port;
        private Task listenerTask;

        public SleepServer()
            : this(9296)
        {
        }

        public SleepServer(int port)
        {
            Contract.Requires<ArgumentOutOfRangeException>(port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort);

            this.port = port;
        }

        protected bool GoToSleep()
        {
            return SleepInvoker.Suspend();
        }

        public void Start()
        {
            if (listenerTask == null || listenerTask.Status != TaskStatus.Running)
            {
                listenerTask = Task.Factory.StartNew(Listener, TaskCreationOptions.LongRunning);
            }
        }

        protected void Listener()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Any;
                listener = new TcpListener(ipAddress, port);
                listener.Start();
                while (true)
                {
                    Socket clientSocket = listener.AcceptSocket();
                    NetworkStream ns = new NetworkStream(clientSocket);
                    byte ch = 0;
                    int i = 0;
                    byte[] cmd = new byte[256];
                    for (i = 0; ch != 13 && i < cmd.Length - 1; i++)
                    {
                        ns.Read(cmd, i, 1);
                    }
                    cmd[i + 1] = 0; // null-terminate for safety

                    // process command
                    char[] separators = { ' ', '\t', '\r', '\n' };
                    string reply = "ERR SERVER\r\n"; // server's reply to client
                    bool noclose = false; // keep connection open and process another command
                    string[] cmdWords = Encoding.ASCII.GetString(cmd, 0, i).Split(separators, 3, StringSplitOptions.RemoveEmptyEntries);
                    switch (cmdWords[0])
                    {
                        case "SLEEP":
                        case "HIBER":
                            // TODO: go to sleep
                            reply = "OK\r\n";
                            break;
                        case "PWROFF":
                        case "REBOOT":
                            reply = "ERR NOTIMPL\r\n";
                            break;
                        case "VER":
                            reply = "VER 1\r\n";
                            noclose = true;
                            break;
                        default:
                            reply = "ERR BADCMD\r\n";
                            break;
                    }
#if DEBUG
                    Console.WriteLine(string.Join(" ", "Debug: Received command:", cmdWords));
#endif

                    // send reply
#if DEBUG
                    Console.WriteLine("Debug: Sending reply: " + reply);
#endif
                    ns.Write(Encoding.ASCII.GetBytes(reply), 0, reply.Length);

                    // close connection
                    if (noclose) continue;
                    clientSocket.Close();

                    // TODO: perform sleep action if command was valid
#if DEBUG
                    Console.WriteLine("Debug: Received valid command: " + cmdWords[0]);
#endif
                }
            }
            catch (SocketException ex)
            {
                Trace.TraceError(String.Format("SleepServer {0}", ex.Message));
                throw new SleepException("socket error", ex);
            }
        }

        public void Stop()
        {
            listener.Stop();
        }
        public void Suspend()
        {
            listener.Stop();
        }

        private void StopListener()
        {
            Stop();
        }

        public void Resume()
        {
            Start();
        }


    }
}
