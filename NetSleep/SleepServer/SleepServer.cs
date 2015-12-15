using System;
using System.Diagnostics;
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

        // interaction strings
        protected const string CRLF = "\r\n";
        protected const string SUCCESS = "OK" + CRLF;
        protected const string NOTIMPL = "ERR NOTIMPL" + CRLF;
        protected const string BADCMD = "ERR BADCMD" + CRLF;
        protected const string PUNT = "ERR SERVER" + CRLF;
        protected const string VERSION = "VER 1" + CRLF;
        protected const string CAPABILITIES = "COMMANDS CAPA HIBER PWROFF QUIT REBOOT SLEEP VER" + CRLF + CRLF;

        public SleepServer()
            : this(9296)
        {
        }

        public SleepServer(int port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentOutOfRangeException("Invalid TCP port: " + port.ToString());
            }
            this.port = port;
        }

        protected bool GoToSleep()
        {
            return SleepInvoker.Suspend();
        }

        protected bool Hibernate()
        {
            return SleepInvoker.Hibernate();
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
                    // TODO: Fork the client handling code to a new thread and go back to listening

#if DEBUG
                    IPEndPoint remote = (IPEndPoint)clientSocket.RemoteEndPoint;
                    string remaddress = remote.Address.ToString();
                    string remport = remote.Port.ToString();
                    Console.WriteLine("Connection from " + remaddress + ":" + remport);
#endif

                    bool close = false; // conversation finished, close connection
                    bool action_sleep = false; // go to sleep after conversation
                    bool action_hiber = false; // hibernate after conversation
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
                        string reply = PUNT; // server's reply to client
                        string[] cmdWords = Encoding.ASCII.GetString(cmd, 0, i).Split(separators, 3, StringSplitOptions.RemoveEmptyEntries);
                        switch (cmdWords[0])
                        {
                            case "SLEEP":
                                action_sleep = true;
                                reply = SUCCESS;
                                close = true;
                                break;
                            case "HIBER":
                                action_hiber = true;
                                reply = SUCCESS;
                                close = true;
                                break;
                            case "PWROFF":
                            case "REBOOT":
                                reply = NOTIMPL;
                                close = true;
                                break;
                            case "VER":
                                reply = VERSION;
                                close = false;
                                break;
                            case "CAPA":
                            case "HELP":
                                reply = CAPABILITIES;
                                close = false;
                                break;
                            case "AUTH":
                            case "HASH":
                                reply = NOTIMPL;
                                close = false;
                                break;
                            case "QUIT":
                                reply = SUCCESS;
                                close = true;
                                break;
                            default:
                                reply = BADCMD;
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

                    // perform an action if one was validated
                    // TODO: Handle this on a separate thread and wrap with calls to Suspend() and Resume()
                    if (action_sleep)
                    {
                        Console.WriteLine("Simulated call to GoToSleep()");
                        //GoToSleep();
                    }
                    else if (action_hiber)
                    {
                        Console.WriteLine("Simulated call to Hibernate()");
                        //Hibernate()
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
