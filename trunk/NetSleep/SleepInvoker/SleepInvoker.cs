using System.Windows.Forms;

namespace DCornell.NetSleep
{
    public class SleepInvoker
    {
        // Mark empty constructor as private - prevents instantiation
        private SleepInvoker() { }

        /// <summary>
        /// Hibernates the system.
        /// </summary>
        /// <param name="force">True to force suspend to hibernation immediately; False (default) to send a suspend request to all running applications</param>
        /// <returns>True if the system is being suspended to hibernate mode, or false if the suspend was canceled.</returns>
        public static bool Hibernate(bool force = false)
        {
            return SuspendToState(PowerState.Hibernate, force);
        }

        /// <summary>
        /// Suspends the system.
        /// </summary>
        /// <param name="force">True to force suspend immediately; False (default) to send a suspend request to all running applications</param>
        /// <returns>True if the system is being suspended, or false if the suspend was canceled.</returns>
        public static bool Suspend(bool force = false)
        {
            return SuspendToState(PowerState.Suspend, force);
        }

        /// <summary>
        /// Suspend the system to the specified PowerState.
        /// </summary>
        /// <param name="state">PowerState indicating the suspend mode to use (currently Suspend or Hibernate)</param>
        /// <param name="force">True to force suspend immediately; False (default) to send a suspend request to all running applications</param>
        /// <returns>True if the system is being suspended to the specified mode, or false if the suspend was canceled.</returns>
        public static bool SuspendToState(PowerState state, bool force)
        {
            return Application.SetSuspendState(state, force, false);
        }
    }
}
