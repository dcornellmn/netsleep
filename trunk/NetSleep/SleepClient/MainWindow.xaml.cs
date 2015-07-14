using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        }

        private void hibernateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void versionButton_Click(object sender, RoutedEventArgs e)
        {

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


            return pass;
        }
    }
}
