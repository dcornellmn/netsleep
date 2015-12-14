using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
                    // TODO: Fork the client handling code to a new thread and go back to listening
                    Socket clientSocket = listener.AcceptSocket();
                    NetworkStream ns = new NetworkStream(clientSocket);

#if DEBUG
                    IPEndPoint remote = (IPEndPoint) clientSocket.RemoteEndPoint;
                    string remaddress = remote.Address.ToString();
                    string remport = remote.Port.ToString();
                    Console.WriteLine("Connection from " + remaddress + ":" + remport);
#endif

                    bool close = false; // conversation finished, close connection
                    bool action_sleep = false; // go to sleep after conversation
                    bool action_reboot = false; // reboot after conversation
                    bool action_shutdown = false; // power down after conversation
                    while (!close)
                    {
                        byte ch = 0;
                        int i = 0;
                        byte[] cmd = new byte[256];
                        for (i = 0; ch != 13 && i < cmd.Length - 1; i++)
                        {
                            // TODO: Wrap this potentially blocking code with some sort of timer
                            ns.Read(cmd, i, 1);
                        }
                        cmd[i + 1] = 0; // null-terminate for safety

                        // process command
                        char[] separators = { ' ', '\t', '\r', '\n' };
                        string reply = "ERR SERVER\r\n"; // server's reply to client
                        string[] cmdWords = Encoding.ASCII.GetString(cmd, 0, i).Split(separators, 3, StringSplitOptions.RemoveEmptyEntries);
                        switch (cmdWords[0])
                        {
                            case "SLEEP":
                            case "HIBER":
                                action_sleep = true;
                                reply = "OK\r\n";
                                close = true;
                                break;
                            case "PWROFF":
                            case "REBOOT":
                                reply = "ERR NOTIMPL\r\n";
                                close = true;
                                break;
                            case "VER":
                                reply = "VER 1\r\n";
                                close = false;
                                break;
                            case "CAPA":
                            case "HELP":
                                reply = "COMMANDS CAPA HIBER PWROFF QUIT REBOOT SLEEP VER\r\n\r\n";
                                close = false;
                                break;
                            case "AUTH":
                            case "HASH":
                                reply = "ERR NOTIMPL\r\n";
                                close = false;
                                break;
                            case "QUIT":
                                reply = "OK\r\n";
                                close = true;
                                break;
                            default:
                                reply = "ERR BADCMD\r\n";
                                close = true;
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
                    }

                    // close connection
#if DEBUG
                    Console.WriteLine("Debug: Closing client connection.");
#endif
                    clientSocket.Close();

                    // TODO: perform sleep action if command was valid
                    if (action_sleep)
                    {
                        Console.WriteLine("Simulated call to GoToSleep()");
                    }
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
