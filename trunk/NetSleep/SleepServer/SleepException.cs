using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCornell.NetSleep
{
    [Serializable]
    public class SleepException : Exception
    {
        public SleepException() { }
        public SleepException(string message) : base(message) { }
        public SleepException(string message, Exception inner) : base(message, inner) { }
        protected SleepException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
