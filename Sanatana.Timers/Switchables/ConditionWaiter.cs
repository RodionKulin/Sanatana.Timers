using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Timers.Switchables
{
    public class ConditionWaiter : IDisposable
    {
        //fields
        protected NonReentrantTimer _timer;
        protected ManualResetEventSlim _stopEventHandle;
        protected Func<bool> _conditionCheck;
     

        //properties
        public virtual TimeSpan CallbackInterval
        {
            get { return _timer.CallbackInterval; }
            set { _timer.CallbackInterval = value; }
        }



        //init
        public ConditionWaiter(Func<bool> conditionCheck)
        {
            _conditionCheck = conditionCheck;

            TimeSpan defaultInterval = TimeSpan.FromMilliseconds(100);
            _timer = new NonReentrantTimer(TimerCallback, defaultInterval, false);
            _stopEventHandle = new ManualResetEventSlim(false);
        }


        //methods
        public virtual bool Wait(TimeSpan? timeout = null)
        {
            if(_timer.IsStarted)
            {
                return _conditionCheck();
            }
            if(_conditionCheck())
            {
                return true;
            }

            _stopEventHandle.Reset();
            _timer.Start();

            if (timeout == null)
                _stopEventHandle.Wait();
            else
                _stopEventHandle.Wait(timeout.Value);

            _timer.Stop();
            return _conditionCheck();
        }

        protected virtual bool TimerCallback()
        {
            bool conditionCompleted = _conditionCheck();
            if (conditionCompleted)
            {
                _stopEventHandle.Set();
                return false;
            }

            return true;
        }

        public virtual void Dispose()
        {
            _stopEventHandle.Dispose();
            _timer.Dispose();
        }
    }
}
