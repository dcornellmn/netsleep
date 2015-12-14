using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SleepClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int port;
        IPAddress ipaddr;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void sleepButton_Click(object sender, RoutedEventArgs e)
        {
            sendCommand("SLEEP\r\n");
        }

        private void hibernateButton_Click(object sender, RoutedEventArgs e)
        {
            sendCommand("HIBER\r\n");
        }

        private void versionButton_Click(object sender, RoutedEventArgs e)
        {
            sendCommand("VER\r\n");
        }

        private void sendCommand(string command)
        {
            try
            {
                // Prevent additional clicks
                sleepButton.IsEnabled = false;
                hibernateButton.IsEnabled = false;
                versionButton.IsEnabled = false;

                resultsTextBox.Text = string.Format("Connecting to %s:%d...\n", ipaddr.ToString(), port);

                // TODO: Add async handling here
                TcpClient tcp = new TcpClient(ipaddr.ToString(), port);
                NetworkStream ns = tcp.GetStream();

                resultsTextBox.Text += string.Format("Connected. Sending %s...\n", command);

                // Send command string (as byte array)
                var cmdbytes = Encoding.ASCII.GetBytes(command);
                ns.Write(cmdbytes, 0, cmdbytes.Length);

                // Listen for response
                byte[] reply = new byte[256];
                ns.Read(reply, 0, 255);

                resultsTextBox.Text += "Response:\n" + Encoding.Unicode.GetString(reply).Trim('\0');
            }
            catch (Exception ex)
            {
                resultsTextBox.Text += "Caught " + ex.GetType().Name;
            }
            finally
            {
                // Re-enable button clicks
                sleepButton.IsEnabled = true;
                hibernateButton.IsEnabled = true;
                versionButton.IsEnabled = true;
            }
        }

        private bool validateInputs()
        {
            string portstr = portTextBox.Text.Trim();
            string addrstr = hostTextBox.Text.Trim();
            string message = string.Empty;
            bool pass = true;

            // Validate port
            if (!string.IsNullOrWhiteSpace(portstr))
            {
                port = 9296;
            }
            else
            {
                if (int.TryParse(portstr, out port))
                {
                    if (port < 1 || port > 65534)
                    {
                        message += "Port must be between 1 and 65534.";
                        pass = false;
                        port = 9296;
                        portTextBox.Text = "9296";
                    }
                }
                else
                {
                    message += "Port was not recognized. Using default.";
                    port = 9296;
                }
            }

            // Validate IP address
            if (!IPAddress.TryParse(addrstr, out ipaddr))
            {
                // TODO: attempt DNS lookup
                message += "Invalid IP address.";
                pass = false;
            }

            return pass;
        }
    }
}
