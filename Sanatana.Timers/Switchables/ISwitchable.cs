using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Timers.Switchables
{
    public interface ISwitchable
    {
        void Start();
        void Stop();
        SwitchState State { get; }
    }
}
