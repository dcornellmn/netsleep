using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCornell.NetSleep.SleepCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            bool force = false;
            bool hiber = false;

            if (args.Length < 1)
            {
                PrintUsage();
            }
            else
            {
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "/f":
                        case "/F":
                            force = true;
                            break;
                        case "/h":
                        case "/H":
                            hiber = true;
                            break;
                        case "/s":
                        case "/S":
                            hiber = false;
                            break;
                        default:
                            Console.WriteLine("Invalid argument: " + arg);
                            PrintUsage();
                            break;
                    }
                }

                // DEBUG OUTPUT
                Console.WriteLine("Preparing to sleep... " + (hiber ? "Hibernate" : "Suspend") + " mode selected, Force is " + (force ? "" : "not ") + "set.");

                bool success;
                if (hiber)
                {
                    success = SleepInvoker.Hibernate(force);
                }
                else { 
                    success = SleepInvoker.Suspend(force);
                }

                if (success)
                {
                    Console.WriteLine("Returned from sleep with success code.");
                }
                else
                {
                    Console.WriteLine("Sleep failed or was canceled.");
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: SleepCLI [/f] {/s | /h}");
            Console.WriteLine("  /f\tForce the system to sleep or hibernate immediately");
            Console.WriteLine("  /h\tSleep the system to Hibernate mode (aka suspend to disk)");
            Console.WriteLine("  /s\tSleep the system to Suspend mode (aka suspend to RAM)");
        }
    }
}
