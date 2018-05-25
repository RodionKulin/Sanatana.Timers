using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sanatana.Timers.Switchables
{
    public class SwitchableRunner
    {
        //fields
        protected List<ISwitchable> _switchables;


        //init
        public SwitchableRunner(IEnumerable<ISwitchable> switchables)
        {
            _switchables = switchables.ToList();
        }


        //methods
        public virtual void Start()
        {
            _switchables.ForEach((s) => s.Start());
        }

        public virtual void Stop(TimeSpan? timeout)
        {
            _switchables.ForEach((s) => s.Stop());

            //block thread until all stopped
            var waiter = new ConditionWaiter(() =>
            {
                return _switchables.All(x => x.State == SwitchState.Stopped);
            });
            bool conditionCompleted = waiter.Wait(timeout);
        }
    }
}
