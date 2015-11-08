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
                    // TODO: Implement network communication
                    NetworkStream ns = new NetworkStream(clientSocket);
                    byte ch = 0;
                    byte[] cmd = new byte[256];
                    for (int i = 0; ch != 13 && i < cmd.Length; i++)
                    {
                        ns.Read(cmd, i, 1);
                    }
                    // process command
                    clientSocket.Close();
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
